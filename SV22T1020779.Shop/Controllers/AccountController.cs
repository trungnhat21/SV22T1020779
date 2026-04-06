using Microsoft.AspNetCore.Mvc;
using SV22T1020779.BusinessLayers;
using SV22T1020779.BusinessLayers.Shop;
using SV22T1020779.Models.AccountCustomer;
using SV22T1020779.Models.Partner;

namespace SV22T1020779.Shop.Controllers
{
    public class AccountController : Controller
    {
        /// <summary>
        /// Hiển thị trang đăng ký tài khoản
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var model = new RegistrationViewModel()
            {
                Provinces = await DictionaryDataService.ListProvincesAsync()
            };
            return View(model);
        }

        /// <summary>
        /// Xử lý đăng ký tài khoản mới cho khách hàng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View(model);
            }

            var customer = new AccountCustomer
            {
                CustomerName = model.CustomerName,
                ContactName = model.ContactName ?? "",
                Province = model.Province ?? "",
                Address = model.Address ?? "",
                Phone = model.Phone ?? "",
                Email = model.Email,
                IsLocked = false,
                Password = SecurityService.ToMD5(model.Password)
            };

            int result = CustomerAccountService.Register(customer);
            if (result == -1)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                model.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View(model);
            }

            return RedirectToAction("Login");
        }

        /// <summary>
        /// Hiển thị trang đăng nhập dành cho khách hàng
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login() => View();

        /// <summary>
        /// Xử lý đăng nhập, kiểm tra thông tin tài khoản và thiết lập Session
        /// </summary>
        /// <param name="email">Email đăng nhập</param>
        /// <param name="password">Mật khẩu</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ Email và Mật khẩu");
                return View();
            }

            var customer = CustomerAccountService.Login(email, password);
            if (customer == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác");
                return View();
            }

            if (customer.IsLocked)
            {
                ModelState.AddModelError("", "Tài khoản của bạn hiện đang bị khóa");
                return View();
            }

            HttpContext.Session.SetInt32("UserId", customer.CustomerID);
            HttpContext.Session.SetString("UserDisplayName", customer.ContactName);
            HttpContext.Session.SetString("UserName", customer.CustomerName);

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Đăng xuất khỏi hệ thống và xóa toàn bộ dữ liệu Session của người dùng
        /// </summary>
        /// <returns></returns>
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Hiển thị thông tin hồ sơ cá nhân của khách hàng đang đăng nhập
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            int? customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null) return RedirectToAction("Login");

            var customer = CustomerAccountService.GetCustomer(customerId.Value);

            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();

            return View(customer);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin cá nhân của khách hàng
        /// </summary>
        /// <param name="data">Dữ liệu khách hàng cần cập nhật</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(AccountCustomer data)
        {
            var currentData = CustomerAccountService.GetCustomer(data.CustomerID);

            if (currentData != null)
            {
                bool isChanged = data.CustomerName != currentData.CustomerName ||
                                 data.ContactName != currentData.ContactName ||
                                 data.Phone != currentData.Phone ||
                                 data.Province != currentData.Province;

                if (isChanged)
                {
                    if (CustomerAccountService.UpdateCustomer(data))
                    {
                        TempData["Message"] = "Cập nhật thông tin thành công!";
                        HttpContext.Session.SetString("UserDisplayName", data.ContactName);
                        HttpContext.Session.SetString("UserName", data.CustomerName);
                    }
                }
            }
            return RedirectToAction("Profile");
        }

        /// <summary>
        /// Hiển thị trang thay đổi mật khẩu
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ChangePassword() => View();

        /// <summary>
        /// Xử lý kiểm tra mật khẩu cũ và thực hiện cập nhật mật khẩu mới cho người dùng
        /// </summary>
        /// <param name="oldPassword">Mật khẩu hiện tại</param>
        /// <param name="newPassword">Mật khẩu mới</param>
        /// <param name="confirmPassword">Nhập lại mật khẩu mới</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            int? customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null) return RedirectToAction("Login");

            var user = CustomerAccountService.GetCustomer(customerId.Value);

            if (string.IsNullOrWhiteSpace(oldPassword))
                ModelState.AddModelError("", "Vui lòng nhập mật khẩu hiện tại");

            if (string.IsNullOrWhiteSpace(newPassword))
                ModelState.AddModelError("", "Vui lòng nhập mật khẩu mới");

            if (!string.IsNullOrEmpty(newPassword) && newPassword.Length < 6)
                ModelState.AddModelError("", "Mật khẩu mới phải có ít nhất 6 ký tự");

            if (ModelState.IsValid)
            {
                if (user == null || user.Password != SecurityService.ToMD5(oldPassword))
                {
                    ModelState.AddModelError("", "Mật khẩu hiện tại không chính xác");
                }

                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("", "Xác nhận mật khẩu mới không khớp");
                }
                if (oldPassword == newPassword)
                {
                    ModelState.AddModelError("", "Mật khẩu mới không được trùng với mật khẩu hiện tại");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            CustomerAccountService.ChangePassword(customerId.Value, newPassword);
            TempData["Message"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile");
        }
    }
}