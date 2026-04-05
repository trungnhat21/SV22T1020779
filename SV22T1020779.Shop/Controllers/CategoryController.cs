using Microsoft.AspNetCore.Mvc;

namespace SV22T1020779.Shop.Controllers
{
    public class CategoryController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}