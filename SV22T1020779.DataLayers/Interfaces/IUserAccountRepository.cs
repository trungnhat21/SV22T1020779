using SV22T1020779.Models.Security;

namespace SV22T1020779.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu liên quan đến tài khoản
    /// </summary>
    public interface IUserAccountRepository
    {
        /// <summary>
        /// Kiểm tra xem tên đăng nhập và mật khẩu có hợp lệ không
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>
        /// Trả về thông tin của tài khoản nếu thông tin đăng nhập hợp lệ,
        /// ngược lại trả về null
        /// </returns>
        Task<UserAccount?> AuthorizeAsync(string userName, string password);
        /// <summary>
        /// Đổi mật khẩu của tài khoản
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> ChangePasswordAsync(string userName, string password);

        /// <summary>
        /// Cập nhật danh sách quyền cho tài khoản.
        /// Chỉ áp dụng cho tài khoản nhân viên (Employee).
        /// Tài khoản khách hàng (Customer) không có chức năng phân quyền —
        /// triển khai mặc định trả về false.
        /// </summary>
        /// <param name="userName">Tên đăng nhập (email) của tài khoản cần cập nhật quyền</param>
        /// <param name="roleNames">
        /// Chuỗi tên các quyền phân cách bởi dấu chấm phẩy,
        /// ví dụ: "Employees;Orders;Products".
        /// Truyền chuỗi rỗng để xóa toàn bộ quyền.
        /// </param>
        /// <returns>true nếu cập nhật thành công</returns>
        Task<bool> ChangeRoleNamesAsync(string userName, string roleNames);
    }
}
