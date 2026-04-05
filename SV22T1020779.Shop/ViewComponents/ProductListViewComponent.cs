using Microsoft.AspNetCore.Mvc;
using SV22T1020779.BusinessLayers;

namespace SV22T1020779.Shop.ViewComponents
{
    public class ProductListViewComponent : ViewComponent
    {
        /// <summary>
        /// Thực thi ViewComponent để lấy danh sách sản phẩm mới nhất hoặc sản phẩm tiêu biểu
        /// từ Database để hiển thị trên các trang như Trang chủ hoặc các khối danh sách sản phẩm.
        /// </summary>
        /// <param name="pageSize">Số lượng sản phẩm tối đa cần hiển thị (mặc định là 9)</param>
        /// <returns>Trả về View kèm theo danh sách sản phẩm (DataItems)</returns>
        public async Task<IViewComponentResult> InvokeAsync(int pageSize = 9)
        {
            var input = new SV22T1020779.Models.Catalog.ProductSearchInput
            {
                Page = 1,
                PageSize = pageSize,
                SearchValue = "",
                CategoryID = 0,
                SupplierID = 0,
                MinPrice = 0,
                MaxPrice = 0
            };

            var result = await CatalogDataService.ListProductsAsync(input);

            return View(result.DataItems);
        }
    }
}