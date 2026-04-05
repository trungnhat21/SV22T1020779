using SV22T1020779.DataLayers;
using SV22T1020779.DataLayers.SqlServer;
using SV22T1020779.Models.Partner;


namespace SV22T1020779.BusinessLayers.Shop
{
    public static class CustomerAccountService
    {
        private static ICustomerAccountDAL customerAccountDAL;

        /// <summary>
        /// Khởi tạo Service với chuỗi kết nối Database
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối SQL Server</param>
        public static void Initialize(string connectionString)
        {
            customerAccountDAL = new CustomerAccountDAL(connectionString);
        }

        /// <summary>
        /// Thực hiện đăng ký tài khoản mới cho khách hàng (Kiểm tra email trùng lặp trước khi đăng ký)
        /// </summary>
        /// <param name="data">Thông tin tài khoản khách hàng</param>
        /// <returns>ID của khách hàng vừa tạo hoặc -1 nếu email đã tồn tại</returns>
        public static int Register(AccountCustomer data)
        {
            if (customerAccountDAL.IsEmailExists(data.Email)) return -1;
            return customerAccountDAL.Register(data);
        }

        /// <summary>
        /// Kiểm tra thông tin đăng nhập của khách hàng
        /// </summary>
        /// <param name="email">Email khách hàng</param>
        /// <param name="password">Mật khẩu (dạng chưa mã hóa)</param>
        /// <returns>Thông tin khách hàng nếu đăng nhập đúng, ngược lại trả về null</returns>
        public static AccountCustomer? Login(string email, string password)
        {
            string encryptedPassword = SecurityService.ToMD5(password);

            return customerAccountDAL.Login(email, encryptedPassword);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một khách hàng dựa trên ID
        /// </summary>
        /// <param name="customerId">Mã khách hàng</param>
        /// <returns>Thông tin AccountCustomer hoặc null</returns>
        public static AccountCustomer? GetCustomer(int customerId) => customerAccountDAL.Get(customerId);

        /// <summary>
        /// Cập nhật thông tin hồ sơ cá nhân của khách hàng
        /// </summary>
        /// <param name="data">Dữ liệu khách hàng cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công</returns>
        public static bool UpdateCustomer(AccountCustomer data) => customerAccountDAL.Update(data);

        /// <summary>
        /// Thực hiện thay đổi mật khẩu cho khách hàng (Mật khẩu mới sẽ được mã hóa MD5)
        /// </summary>
        /// <param name="customerId">Mã khách hàng</param>
        /// <param name="newPassword">Mật khẩu mới (dạng chưa mã hóa)</param>
        /// <returns>True nếu đổi mật khẩu thành công</returns>
        public static bool ChangePassword(int customerId, string newPassword)
        {
            string encryptedPassword = SecurityService.ToMD5(newPassword);
            return customerAccountDAL.ChangePassword(customerId, encryptedPassword);
        }
    }
}