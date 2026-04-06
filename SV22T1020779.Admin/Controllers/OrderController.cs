using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SV22T1020779.BusinessLayers;
using SV22T1020779.Models.Catalog;
using SV22T1020779.Models.Common;
using SV22T1020779.Models.Sales;

namespace SV22T1020779.Admin.Controllers
{
    /// <summary>
    /// Controller điều khiển các hoạt động liên quan đến quản lý và lập đơn hàng
    /// </summary>
    public class OrderController : Controller
    {
        private const string SEARCH_PRODUCT = "SearchProduct";
        public const int PAGESIZE = 10;
        public const string SEARCH_ORDER = "SearchOrder";
        private const string CART_KEY = "AdminOrderCart";

        /// <summary>
        /// Hiển thị giao diện danh sách đơn hàng
        /// </summary>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<OrderSearchInput>(SEARCH_ORDER);
            if (input == null)
            {
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = "",
                    Status = 0,
                    DateFrom = null,
                    DateTo = null
                };
            }
            return View(input);
        }

        /// <summary>
        /// Tìm kiếm đơn hàng dựa trên các tiêu chí lọc
        /// </summary>
        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            if (input == null)
            {
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = "",
                    Status = 0
                };
            }

            var result = await SalesDataService.ListOrdersAsync(input);
            return View(result);
        }

        /// <summary>
        /// Hiển thị giao diện lập đơn hàng mới
        /// </summary>
        public async Task<IActionResult> Create(string searchValue = "")
        {
            var input = new ProductSearchInput()
            {
                Page = 1,
                PageSize = 10,
                SearchValue = searchValue ?? ""
            };

            var result = await CatalogDataService.ListProductsAsync(input);
            return View(result.DataItems);
        }

        /// <summary>
        /// Tìm kiếm mặt hàng để thêm vào đơn hàng
        /// </summary>
        public async Task<IActionResult> SearchProduct(ProductSearchInput input)
        {
            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(SEARCH_PRODUCT, input);
            return View(result);
        }

        /// <summary>
        /// Xem thông tin chi tiết của một đơn hàng
        /// </summary>
        public async Task<IActionResult> Detail(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return RedirectToAction("Index");

            order.Details = await SalesDataService.ListDetailsAsync(id);
            return View(order);
        }

        /// <summary>
        /// Duyệt đơn hàng (Chuyển sang trạng thái đã tiếp nhận)
        /// </summary>
        public async Task<IActionResult> Accept(int id)
        {
            int employeeID = 1;
            bool result = await SalesDataService.AcceptOrderAsync(id, employeeID);
            if (result)
                TempData["Message"] = "Đã duyệt đơn hàng thành công.";
            else
                TempData["Error"] = "Không thể duyệt đơn hàng này.";

            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Lấy giao diện chọn người giao hàng (Shipper) cho đơn hàng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Shipping(int id)
        {
            if (id <= 0) return NotFound();

            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null) return NotFound();

            ViewBag.OrderID = id;

            var input = new SV22T1020779.Models.Common.PaginationSearchInput { Page = 1, PageSize = 100, SearchValue = "" };
            var result = await PartnerDataService.ListShippersAsync(input);

            return PartialView(result.DataItems);
        }

        /// <summary>
        /// Xác nhận chuyển đơn hàng cho người giao hàng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Shipping(int id, int shipperID = 0)
        {
            if (id <= 0) return NotFound();

            if (shipperID <= 0)
            {
                TempData["Error"] = "Vui lòng chọn một đơn vị vận chuyển từ danh sách.";
                return RedirectToAction("Detail", new { id = id });
            }

            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("Index");
            }

            bool result = await SalesDataService.ShipOrderAsync(id, shipperID);

            if (result)
                TempData["Message"] = $"Đơn hàng #{id} đã được chuyển cho đơn vị vận chuyển.";
            else
                TempData["Error"] = "Cập nhật thất bại. Vui lòng kiểm tra lại.";

            return RedirectToAction("Detail", new { id = id });
        }

        /// <summary>
        /// Xác nhận hoàn tất đơn hàng
        /// </summary>
        public async Task<IActionResult> Finish(int id)
        {
            bool result = await SalesDataService.CompleteOrderAsync(id);
            if (result)
                TempData["Message"] = "Đơn hàng đã hoàn tất thành công.";
            else
                TempData["Error"] = "Không thể hoàn tất đơn hàng.";

            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Từ chối đơn hàng
        /// </summary>
        public async Task<IActionResult> Reject(int id)
        {
            int employeeID = 1;
            bool result = await SalesDataService.RejectOrderAsync(id);
            if (result)
                TempData["Message"] = "Đã từ chối đơn hàng.";
            else
                TempData["Error"] = "Không thể từ chối đơn hàng này.";

            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Hủy bỏ đơn hàng
        /// </summary>
        public async Task<IActionResult> Cancel(int id)
        {
            bool result = await SalesDataService.CancelOrderAsync(id);
            if (result)
            {
                TempData["Message"] = "Đã hủy đơn hàng thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể hủy đơn hàng này. Vui lòng kiểm tra lại trạng thái.";
            }
            return RedirectToAction("Detail", new { id = id });
        }

        /// <summary>
        /// Xóa vĩnh viễn đơn hàng khỏi hệ thống
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            bool result = await SalesDataService.DeleteOrderAsync(id);
            if (result)
                TempData["Message"] = "Đã xóa đơn hàng thành công.";
            else
                TempData["Error"] = "Không thể xóa đơn hàng (Chỉ được xóa đơn vừa khởi tạo).";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Hiển thị giao diện chỉnh sửa mặt hàng trong giỏ hàng
        /// </summary>
        /// <summary>
        /// Hiển thị giao diện chỉnh sửa mặt hàng trong đơn hàng (Modal)
        /// </summary>
        public async Task<IActionResult> EditCartItem(int id = 0, int productId = 0)
        {
            var model = await SalesDataService.GetDetailAsync(id, productId);
            if (model == null)
                return RedirectToAction("Detail", new { id });

            return PartialView(model);
        }

        /// <summary>
        /// Cập nhật chi tiết đơn hàng và quay lại trang chi tiết
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(int orderID, int productID, int quantity, decimal salePrice)
        {
            if (quantity <= 0)
            {
                TempData["Error"] = "Số lượng phải lớn hơn 0";
                return RedirectToAction("Detail", new { id = orderID });
            }

            bool result = await SalesDataService.SaveOrderDetailAsync(orderID, productID, quantity, salePrice);
            if (!result)
                TempData["Error"] = "Không thể cập nhật mặt hàng. Có thể đơn hàng đã được duyệt hoặc chuyển giao.";

            return RedirectToAction("Detail", new { id = orderID });
        }

        /// <summary>
        /// Xóa một mặt hàng khỏi đơn hàng đã lưu trong Database
        /// </summary>
        public async Task<IActionResult> DeleteCartItem(int id, int productId)
        {
            bool result = await SalesDataService.DeleteDetailAsync(id, productId);
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Lấy danh sách giỏ hàng hiện tại từ Session
        /// </summary>
        public IActionResult GetCart()
        {
            var cart = GetCartFromSession();
            return PartialView("Cart", cart);
        }

        /// <summary>
        /// Thêm mặt hàng vào giỏ hàng Session
        /// </summary>
        [HttpPost]
        public IActionResult AddToCart(CartItem item)
        {
            var cart = GetCartFromSession();
            var exists = cart.FirstOrDefault(m => m.ProductID == item.ProductID);
            if (exists != null)
            {
                exists.Quantity += item.Quantity;
                exists.SalePrice = item.SalePrice;
            }
            else
            {
                cart.Add(item);
            }
            SaveCartToSession(cart);
            return PartialView("Cart", cart);
        }

        private List<CartItem> GetCartFromSession()
        {
            var session = HttpContext.Session.GetString(CART_KEY);
            return session != null ? JsonConvert.DeserializeObject<List<CartItem>>(session) : new List<CartItem>();
        }

        private void SaveCartToSession(List<CartItem> cart) =>
            HttpContext.Session.SetString(CART_KEY, JsonConvert.SerializeObject(cart));

        /// <summary>
        /// Xóa sạch giỏ hàng trong Session
        /// </summary>
        [HttpPost]
        public IActionResult ClearCart()
        {
            var cart = new List<CartItem>();
            SaveCartToSession(cart);
            return PartialView("Cart", cart);
        }

        /// <summary>
        /// Khởi tạo đơn hàng từ giỏ hàng Session vào Database
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> InitOrder(string customerName, string deliveryProvince, string deliveryAddress)
        {
            var cart = GetCartFromSession();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống, không thể lập đơn hàng.";
                return RedirectToAction("Create");
            }

            if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(deliveryProvince))
            {
                TempData["Error"] = "Vui lòng nhập tên khách hàng và chọn tỉnh thành.";
                return RedirectToAction("Create");
            }

            var input = new PaginationSearchInput()
            {
                Page = 1,
                PageSize = 100,
                SearchValue = customerName ?? ""
            };

            var customersResult = await PartnerDataService.ListCustomersAsync(input);

            var customer = customersResult.DataItems.FirstOrDefault(c =>
                c.CustomerName.Trim().ToLower() == customerName.Trim().ToLower());

            int? customerID = customer?.CustomerID;

            int employeeID = 1;

            var orderDetails = cart.Select(item => new OrderDetail()
            {
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                SalePrice = item.SalePrice
            }).ToList();

            int orderID = await SalesDataService.InitOrderAsync(employeeID, customerID, deliveryProvince, deliveryAddress, orderDetails);

            if (orderID > 0)
            {
                HttpContext.Session.Remove(CART_KEY);
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Không thể lập đơn hàng. Vui lòng thử lại.";
            return RedirectToAction("Create");
        }

        /// <summary>
        /// Giảm số lượng của một mặt hàng trong giỏ hàng Session
        /// </summary>
        public IActionResult DecreaseQuantity(int id)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(m => m.ProductID == id);

            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity -= 1;
                }
                else
                {
                    cart.Remove(item);
                }
                SaveCartToSession(cart);
            }
            return RedirectToAction("Create");
        }



    }
}