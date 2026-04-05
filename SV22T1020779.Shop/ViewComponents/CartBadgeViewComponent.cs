using Microsoft.AspNetCore.Mvc;

namespace SV22T1020779.Shop.Components
{
    public class CartBadgeViewComponent : ViewComponent
    {
        /// <summary>
        /// Thực thi ViewComponent để tính toán số lượng mặt hàng hiện có trong giỏ hàng 
        /// (Dựa trên đơn hàng ở trạng thái Vừa khởi tạo của người dùng) để hiển thị lên Badge icon
        /// </summary>
        /// <returns>Trả về View kèm theo con số số lượng mặt hàng (count)</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            int count = HttpContext.Session.GetInt32("CartCount") ?? 0;
            return View(count);
        }
    }
}