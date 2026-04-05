using Microsoft.AspNetCore.Mvc;
using SV22T1020779.BusinessLayers;
using SV22T1020779.Models.Common;

namespace SV22T1020779.Shop.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        /// <summary>
        /// Thực thi ViewComponent để lấy danh sách các loại hàng (Categories) từ Database
        /// dùng để hiển thị menu danh mục sản phẩm trên giao diện người dùng
        /// </summary>
        /// <returns>Trả về View kèm theo danh sách các loại hàng (DataItems)</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {

            var input = new PaginationSearchInput { Page = 1, PageSize = 100, SearchValue = "" };
            var result = await CatalogDataService.ListCategoriesAsync(input);


            return View(result.DataItems);
        }
    }
}