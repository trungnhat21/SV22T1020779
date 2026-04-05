using SV22T1020779.Models.Partner;

namespace SV22T1020779.DataLayers
{
    public interface ICustomerAccountDAL
    {
        int Register(AccountCustomer data);
        bool IsEmailExists(string email);
        AccountCustomer? Login(string email, string password);
        AccountCustomer? Get(int customerId);
        bool Update(AccountCustomer data);
        bool ChangePassword(int customerId, string newPassword);
    }
}