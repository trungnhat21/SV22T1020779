using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020779.Models.Partner;

namespace SV22T1020779.DataLayers.SqlServer
{
    public class CustomerAccountDAL : ICustomerAccountDAL
    {
        private readonly string _connectionString;
        public CustomerAccountDAL(string connectionString) => _connectionString = connectionString;

        /// <summary>
        /// Thêm mới một khách hàng vào bảng Customers và trả về ID vừa tạo
        /// </summary>
        /// <param name="data">Đối tượng khách hàng cần lưu</param>
        /// <returns>ID của khách hàng (CustomerID) được sinh tự động</returns>
        public int Register(AccountCustomer data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var sql = @"INSERT INTO Customers(CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked)
                            VALUES(@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @Password, @IsLocked);
                            SELECT SCOPE_IDENTITY();";
                return connection.ExecuteScalar<int>(sql, data);
            }
        }

        /// <summary>
        /// Kiểm tra xem địa chỉ Email đã tồn tại trong hệ thống hay chưa
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <returns>True nếu email đã tồn tại</returns>
        public bool IsEmailExists(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Customers WHERE Email = @Email", new { Email = email }) > 0;
            }
        }

        /// <summary>
        /// Truy vấn thông tin khách hàng dựa trên Email và Mật khẩu (dùng cho đăng nhập)
        /// </summary>
        /// <param name="email">Email đăng nhập</param>
        /// <param name="password">Mật khẩu đã mã hóa</param>
        /// <returns>Thông tin khách hàng hoặc null nếu sai thông tin</returns>
        public AccountCustomer? Login(string email, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var sql = @"SELECT * FROM Customers 
                    WHERE Email = @Email AND Password = @Password";

                return connection.QueryFirstOrDefault<AccountCustomer>(sql, new
                {
                    Email = email,
                    Password = password
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một khách hàng dựa trên ID
        /// </summary>
        /// <param name="customerId">Mã khách hàng</param>
        /// <returns>Đối tượng AccountCustomer hoặc null</returns>
        public AccountCustomer? Get(int customerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var sql = @"SELECT * FROM Customers WHERE CustomerID = @CustomerID";
                return connection.QueryFirstOrDefault<AccountCustomer>(sql, new { CustomerID = customerId });
            }
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân của khách hàng vào Database
        /// </summary>
        /// <param name="data">Dữ liệu khách hàng mới</param>
        /// <returns>True nếu cập nhật thành công ít nhất 1 dòng</returns>
        public bool Update(AccountCustomer data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var sql = @"UPDATE Customers 
                    SET CustomerName = @CustomerName, 
                        ContactName = @ContactName,
                        Province = @Province,
                        Address = @Address,
                        Phone = @Phone,
                        Email = @Email
                    WHERE CustomerID = @CustomerID";
                return connection.Execute(sql, data) > 0;
            }
        }

        /// <summary>
        /// Cập nhật mật khẩu mới cho khách hàng theo ID
        /// </summary>
        /// <param name="customerId">Mã khách hàng</param>
        /// <param name="newPassword">Mật khẩu mới đã mã hóa</param>
        /// <returns>True nếu cập nhật thành công</returns>
        public bool ChangePassword(int customerId, string newPassword)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var sql = @"UPDATE Customers SET Password = @Password WHERE CustomerID = @CustomerID";
                return connection.Execute(sql, new { Password = newPassword, CustomerID = customerId }) > 0;
            }
        }
    }
}