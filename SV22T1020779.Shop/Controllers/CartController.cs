using Microsoft.AspNetCore.Mvc;
using SV22T1020779.BusinessLayers;
using SV22T1020779.Models.Sales;

namespace SV22T1020779.Shop.Controllers
{
    public class CartController : Controller
    {
        /// <summary>
        /// Hiển thị danh sách sản phẩm trong giỏ hàng (Đơn hàng trạng thái -5)
        /// </summary>
        public async Task<IActionResult> Index()
        {
            int? customerID = HttpContext.Session.GetInt32("UserId");
            if (customerID == null) return RedirectToAction("Login", "Account");

            var cart = await SalesDataService.ListCartAsync(customerID.Value);

            int totalQuantity = cart.Sum(i => i.Quantity);
            HttpContext.Session.SetInt32("CartCount", totalQuantity);

            return View(cart);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng Database
        /// </summary>
        /// <param name="productID">Mã sản phẩm</param>
        /// <param name="quantity">Số lượng (mặc định là 1)</param>
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productID, int quantity = 1)
        {
            int? customerID = HttpContext.Session.GetInt32("UserId");
            if (customerID == null)
            {
                TempData["Message"] = "Vui lòng đăng nhập để thực hiện chức năng mua hàng";
                return RedirectToAction("Login", "Account");
            }

            var product = await CatalogDataService.GetProductAsync(productID);
            if (product == null) return RedirectToAction("Index");

            await SalesDataService.AddToCartAsync(customerID.Value, productID, quantity, product.Price);

            var cart = await SalesDataService.ListCartAsync(customerID.Value);
            HttpContext.Session.SetInt32("CartCount", cart.Sum(i => i.Quantity));

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Cập nhật số lượng mới cho một sản phẩm trong giỏ hàng
        /// </summary>
        /// <param name="productID">Mã sản phẩm</param>
        /// <param name="quantity">Số lượng mới</param>
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productID, int quantity)
        {
            int? customerID = HttpContext.Session.GetInt32("UserId");
            if (customerID != null)
            {
                var product = await CatalogDataService.GetProductAsync(productID);
                if (product != null)
                {
                    var cartOrder = await SalesDataService.GetCartOrderAsync(customerID.Value);
                    if (cartOrder != null)
                    {
                        var detail = await SalesDataService.GetDetailAsync(cartOrder.OrderID, productID);
                        if (detail != null)
                        {
                            detail.Quantity = quantity;
                            if (detail.Quantity > 0)
                                await SalesDataService.UpdateDetailAsync(detail);
                            else
                                await SalesDataService.DeleteDetailAsync(cartOrder.OrderID, productID);
                        }
                    }
                }

                var cart = await SalesDataService.ListCartAsync(customerID.Value);
                HttpContext.Session.SetInt32("CartCount", cart.Sum(i => i.Quantity));
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xác nhận đặt hàng và chuyển đổi giỏ hàng sang đơn hàng chính thức
        /// </summary>
        /// <param name="Address">Địa chỉ giao hàng</param>
        /// <param name="Province">Tỉnh/Thành phố</param>
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(string Address, string Province)
        {
            int? customerID = HttpContext.Session.GetInt32("UserId");
            if (customerID == null) return RedirectToAction("Login", "Account");

            var cart = await SalesDataService.ListCartAsync(customerID.Value);
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index");
            }

            var orderDetails = cart.Select(item => new OrderDetail
            {
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                SalePrice = item.SalePrice
            }).ToList();

            int employeeID = 1;
            int orderID = await SalesDataService.InitOrderAsync(employeeID, customerID.Value, Province, Address, orderDetails);

            if (orderID > 0)
            {
                HttpContext.Session.SetInt32("CartCount", 0);
                TempData["Message"] = "Đặt hàng thành công!";
                return RedirectToAction("MyOrders");
            }

            TempData["Error"] = "Có lỗi xảy ra trong quá trình xử lý đơn hàng.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xóa bỏ một mặt hàng khỏi giỏ hàng
        /// </summary>
        /// <param name="productID">Mã sản phẩm cần xóa</param>
        public async Task<IActionResult> Remove(int productID)
        {
            int? customerID = HttpContext.Session.GetInt32("UserId");
            if (customerID != null)
            {
                var cartOrder = await SalesDataService.GetCartOrderAsync(customerID.Value);
                if (cartOrder != null)
                {
                    await SalesDataService.DeleteDetailAsync(cartOrder.OrderID, productID);

                    var details = await SalesDataService.ListDetailsAsync(cartOrder.OrderID);
                    HttpContext.Session.SetInt32("CartCount", details.Sum(i => i.Quantity));
                }
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Hiển thị danh sách các đơn hàng đã đặt của khách hàng hiện tại
        /// </summary>
        public async Task<IActionResult> MyOrders()
        {
            int? customerID = HttpContext.Session.GetInt32("UserId");
            if (customerID == null) return RedirectToAction("Login", "Account");

            var input = new OrderSearchInput
            {
                CustomerID = customerID.Value,
                Page = 1,
                PageSize = 100,
                Status = 0
            };

            var result = await SalesDataService.ListOrdersAsync(input);
            var displayOrders = result.DataItems.Where(o => (int)o.Status != -5).ToList();

            return View(displayOrders);
        }

        /// <summary>
        /// Xem chi tiết thông tin của một đơn hàng cụ thể
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null) return RedirectToAction("MyOrders");

            var details = await SalesDataService.ListDetailsAsync(id);
            ViewBag.Order = order;
            return View(details);
        }

        /// <summary>
        /// Hủy một đơn hàng đang chờ duyệt
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            bool result = await SalesDataService.DeleteOrderAsync(id);
            if (result) TempData["Message"] = "Hủy đơn hàng thành công!";
            else TempData["Error"] = "Không thể hủy đơn hàng này.";
            return RedirectToAction("MyOrders");
        }

        /// <summary>
        /// Xem lịch sử các đơn hàng đã hoàn tất giao hàng
        /// </summary>
        public async Task<IActionResult> OrderHistory()
        {
            int? customerID = HttpContext.Session.GetInt32("UserId");
            if (customerID == null) return RedirectToAction("Login", "Account");

            var input = new OrderSearchInput { CustomerID = customerID.Value, Page = 1, PageSize = 100 };
            var result = await SalesDataService.ListOrdersAsync(input);

            var completedOrders = result.DataItems
                                        .Where(m => m.Status == OrderStatusEnum.Completed)
                                        .ToList();

            return View(completedOrders);
        }
    }
}