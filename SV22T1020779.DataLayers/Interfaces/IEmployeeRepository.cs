using SV22T1020779.Models.HR;

namespace SV22T1020779.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu trên Employee
    /// </summary>
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        /// <summary>
        /// Kiểm tra xem email của nhân viên có hợp lệ không
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// Nếu id = 0: Kiểm tra email của nhân viên mới
        /// Nếu id <> 0: Kiểm tra email của nhân viên có mã là id
        /// </param>
        /// <returns></returns>
        Task<bool> ValidateEmailAsync(string email, int id = 0);
        //Task<bool> ChangeRolesAsync(int employeeID, string roleNames);

        /// <summary>
        /// Lấy danh sách tên quyền của nhân viên theo mã nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên</param>
        /// <returns>Danh sách tên quyền (tách từ chuỗi RoleNames)</returns>
        Task<List<string>> GetRoleNamesAsync(int id);
    }
}
