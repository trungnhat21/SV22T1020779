using Microsoft.AspNetCore.Mvc;
using SV22T1020779.BusinessLayers;
using SV22T1020779.Models.Catalog;

namespace SV22T1020779.Shop.Controllers
{
    public class ProductController : Controller
    {
        /// <summary>
        /// Tìm kiếm và lọc danh sách sản phẩm dựa trên các tiêu chí: tên, khoảng giá, loại hàng và phân trang
        /// </summary>
        /// <param name="page">Trang hiện tại</param>
        /// <param name="pageSize">Số lượng sản phẩm trên mỗi trang</param>
        /// <param name="searchValue">Từ khóa tìm kiếm theo tên sản phẩm</param>
        /// <param name="minPrice">Giá thấp nhất trong khoảng lọc</param>
        /// <param name="maxPrice">Giá cao nhất trong khoảng lọc</param>
        /// <param name="categoryID">Mã loại hàng cần lọc</param>
        /// <returns></returns>
        public async Task<IActionResult> Search(int page = 1, int pageSize = 24, string searchValue = "", decimal minPrice = 0, decimal maxPrice = 0, int categoryID = 0)
        {
            var input = new ProductSearchInput()
            {
                Page = page,
                PageSize = pageSize,
                SearchValue = searchValue ?? "",
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                CategoryID = categoryID,
                SupplierID = 0
            };

            var result = await CatalogDataService.ListProductsAsync(input);
            return View(result);
        }

        /// <summary>
        /// Xem thông tin chi tiết của một sản phẩm cụ thể theo mã ID
        /// </summary>
        /// <param name="id">Mã sản phẩm cần xem chi tiết</param>
        /// <returns>Nếu không tìm thấy sản phẩm sẽ quay về trang chủ, ngược lại hiển thị view chi tiết</returns>
        public async Task<IActionResult> Detail(int id = 0)
        {
            var product = await CatalogDataService.GetProductAsync(id);

            if (product == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(product);
        }
    }
}