using SV22T1020779.DataLayers.Interfaces;
using SV22T1020779.DataLayers.SQLServer;
using SV22T1020779.Models.Common;
using SV22T1020779.Models.Partner;
using SV22T1020779.Models.Sales;

namespace SV22T1020779.BusinessLayers
{
    /// <summary>
    /// Lớp cung cấp các dịch vụ xử lý dữ liệu liên quan đến bán hàng, đơn hàng và giỏ hàng
    /// </summary>
    public static class SalesDataService
    {
        private static readonly IOrderRepository orderDB;
        private const int SHOPPING_CART_STATUS = -5;

        static SalesDataService()
        {
            orderDB = new OrderRepository(Configuration.ConnectionString);
        }

        #region Giỏ hàng Database (Status = -5)

        /// <summary>
        /// Lấy danh sách các mặt hàng trong giỏ hàng hiện tại của khách hàng
        /// </summary>
        public static async Task<List<OrderDetailViewInfo>> ListCartAsync(int customerID)
        {
            var cartOrder = await GetCartOrderAsync(customerID);
            if (cartOrder == null) return new List<OrderDetailViewInfo>();

            return await orderDB.ListDetailsAsync(cartOrder.OrderID);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng. Nếu chưa có đơn hàng trạng thái -5 sẽ tự động tạo mới.
        /// </summary>
        public static async Task<bool> AddToCartAsync(int customerID, int productID, int quantity, decimal salePrice)
        {
            var cartOrder = await GetCartOrderAsync(customerID);
            int orderID;

            if (cartOrder == null)
            {
                orderID = await orderDB.AddAsync(new Order
                {
                    CustomerID = customerID,
                    OrderTime = DateTime.Now,
                    Status = (OrderStatusEnum)SHOPPING_CART_STATUS,
                    DeliveryProvince = "",
                    DeliveryAddress = ""
                });
            }
            else
            {
                orderID = cartOrder.OrderID;
            }

            var detail = await orderDB.GetDetailAsync(orderID, productID);
            if (detail != null)
            {
                detail.Quantity += quantity;
                detail.SalePrice = salePrice;
                return await orderDB.UpdateDetailAsync(detail);
            }

            return await orderDB.AddDetailAsync(new OrderDetail
            {
                OrderID = orderID,
                ProductID = productID,
                Quantity = quantity,
                SalePrice = salePrice
            });
        }

        /// <summary>
        /// Xóa bỏ hoàn toàn giỏ hàng của khách hàng khỏi hệ thống
        /// </summary>
        public static async Task<bool> ClearCartAsync(int customerID)
        {
            var cartOrder = await GetCartOrderAsync(customerID);
            if (cartOrder != null)
                return await orderDB.DeleteAsync(cartOrder.OrderID);
            return false;
        }

        /// <summary>
        /// Tìm kiếm thông tin đơn hàng đang đóng vai trò là giỏ hàng của khách hàng
        /// </summary>
        public static async Task<OrderViewInfo?> GetCartOrderAsync(int customerID)
        {
            var input = new OrderSearchInput
            {
                Status = (OrderStatusEnum)SHOPPING_CART_STATUS,
                PageSize = 1000
            };
            var result = await orderDB.ListAsync(input);
            return result.DataItems.FirstOrDefault(o => o.CustomerID == customerID);
        }

        #endregion

        #region Order

        /// <summary>
        /// Tìm kiếm và lấy danh sách đơn hàng có phân trang và tính toán tổng tiền
        /// </summary>
        public static async Task<PagedResult<OrderViewInfo>> ListOrdersAsync(OrderSearchInput input)
        {
            var result = await orderDB.ListAsync(input) ?? new PagedResult<OrderViewInfo>()
            {
                Page = 1,
                PageSize = 10,
                RowCount = 0,
                DataItems = new List<OrderViewInfo>()
            };

            if (result.DataItems != null)
            {
                foreach (var order in result.DataItems)
                {
                    var details = await orderDB.ListDetailsAsync(order.OrderID) ?? new List<OrderDetailViewInfo>();
                    order.TotalPrice = details.Sum(x => x.SalePrice * x.Quantity);
                }
            }

            return result;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một đơn hàng theo mã đơn hàng
        /// </summary>
        public static async Task<OrderViewInfo?> GetOrderAsync(int orderID)
        {
            return await orderDB.GetAsync(orderID);
        }

        /// <summary>
        /// Thêm mới một đơn hàng vào hệ thống (mặc định trạng thái đơn hàng mới)
        /// </summary>
        public static async Task<int> AddOrderAsync(Order data)
        {
            data.Status = OrderStatusEnum.New;
            data.OrderTime = DateTime.Now;
            return await orderDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin đơn hàng (chỉ cho phép khi đơn hàng mới hoặc đang là giỏ hàng)
        /// </summary>
        public static async Task<bool> UpdateOrderAsync(Order data)
        {
            var order = await orderDB.GetAsync(data.OrderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && (int)order.Status != SHOPPING_CART_STATUS)
                return false;

            return await orderDB.UpdateAsync(data);
        }

        /// <summary>
        /// Xóa đơn hàng khỏi hệ thống dựa trên trạng thái cho phép xóa
        /// </summary>
        public static async Task<bool> DeleteOrderAsync(int orderID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status == OrderStatusEnum.New ||
                order.Status == OrderStatusEnum.Rejected ||
                order.Status == OrderStatusEnum.Cancelled ||
                (int)order.Status == SHOPPING_CART_STATUS)
            {
                return await orderDB.DeleteAsync(orderID);
            }

            return false;
        }

        #endregion

        #region Order Status Processing

        /// <summary>
        /// Tiếp nhận đơn hàng và gán nhân viên xử lý cùng người giao hàng ngẫu nhiên
        /// </summary>
        public static async Task<bool> AcceptOrderAsync(int orderID, int employeeID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.New) return false;

            if (order.ShipperID == null || order.ShipperID == 0)
            {
                var randomShipper = await PartnerDataService.GetRandomShipperAsync();
                if (randomShipper != null)
                {
                    order.ShipperID = randomShipper.ShipperID;
                }
            }

            order.EmployeeID = employeeID;
            order.AcceptTime = DateTime.Now;
            order.Status = OrderStatusEnum.Accepted;

            return await orderDB.UpdateAsync(order);
        }

        /// <summary>
        /// Từ chối đơn hàng
        /// </summary>
        public static async Task<bool> RejectOrderAsync(int orderID)
        {
            var data = await orderDB.GetAsync(orderID);
            if (data == null) return false;

            data.Status = OrderStatusEnum.Rejected;
            data.ShipperID = null;
            data.FinishedTime = DateTime.Now;

            return await orderDB.UpdateAsync(data);
        }

        /// <summary>
        /// Hủy bỏ đơn hàng nếu đơn hàng chưa hoàn thành hoặc đã bị từ chối
        /// </summary>
        public static async Task<bool> CancelOrderAsync(int orderID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status == OrderStatusEnum.Completed ||
                order.Status == OrderStatusEnum.Rejected ||
                order.Status == OrderStatusEnum.Cancelled)
            {
                return false;
            }

            order.Status = OrderStatusEnum.Cancelled;
            return await orderDB.UpdateAsync(order);
        }

        /// <summary>
        /// Chuyển trạng thái đơn hàng sang đang giao hàng
        /// </summary>
        public static async Task<bool> ShipOrderAsync(int orderID, int shipperID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.Accepted) return false;

            order.ShipperID = shipperID;
            order.ShippedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Shipping;

            return await orderDB.UpdateAsync(order);
        }

        /// <summary>
        /// Xác nhận đơn hàng đã hoàn tất giao hàng
        /// </summary>
        public static async Task<bool> CompleteOrderAsync(int orderID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.Shipping) return false;

            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Completed;

            return await orderDB.UpdateAsync(order);
        }

        #endregion

        #region Order Detail

        /// <summary>
        /// Lấy danh sách chi tiết các mặt hàng trong một đơn hàng
        /// </summary>
        public static async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            return await orderDB.ListDetailsAsync(orderID);
        }

        /// <summary>
        /// Thêm một mặt hàng mới vào chi tiết đơn hàng
        /// </summary>
        public static async Task<bool> AddDetailAsync(OrderDetail data)
        {
            var order = await orderDB.GetAsync(data.OrderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && (int)order.Status != SHOPPING_CART_STATUS)
                return false;

            return await orderDB.AddDetailAsync(data);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một sản phẩm cụ thể trong đơn hàng
        /// </summary>
        public static async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            return await orderDB.GetDetailAsync(orderID, productID);
        }

        /// <summary>
        /// Cập nhật thông tin (số lượng, giá bán) của một mặt hàng trong đơn hàng
        /// </summary>
        public static async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            var order = await orderDB.GetAsync(data.OrderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && (int)order.Status != SHOPPING_CART_STATUS)
                return false;

            return await orderDB.UpdateDetailAsync(data);
        }

        /// <summary>
        /// Xóa bỏ một mặt hàng khỏi đơn hàng
        /// </summary>
        public static async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && (int)order.Status != SHOPPING_CART_STATUS)
                return false;

            return await orderDB.DeleteDetailAsync(orderID, productID);
        }

        /// <summary>
        /// Khởi tạo đơn hàng chính thức từ dữ liệu danh sách sản phẩm và dọn dẹp giỏ hàng cũ
        /// </summary>
        public static async Task<int> InitOrderAsync(int employeeID, int? customerID, string deliveryProvince, string deliveryAddress, List<OrderDetail> orderDetails)
        {
            var orderData = new Order()
            {
                OrderTime = DateTime.Now,
                EmployeeID = employeeID,
                CustomerID = customerID,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = deliveryAddress,
                Status = OrderStatusEnum.New
            };

            int orderID = await orderDB.AddAsync(orderData);

            if (orderID > 0)
            {
                foreach (var item in orderDetails)
                {
                    item.OrderID = orderID;
                    await orderDB.AddDetailAsync(item);
                }

                if (customerID.HasValue)
                {
                    await ClearCartAsync(customerID.Value);
                }

                return orderID;
            }

            return 0;
        }

        #endregion
    }
}