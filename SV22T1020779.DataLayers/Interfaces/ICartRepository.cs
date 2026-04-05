using SV22T1020779.Models.Sales;

namespace SV22T1020779.DataLayers.Interfaces
{
    public interface ICartRepository
    {
        Task<List<OrderDetailViewInfo>> ListAsync(int customerID);
        Task<bool> AddOrUpdateAsync(int customerID, int productID, int quantity);
        Task<bool> DeleteAsync(int customerID, int productID);
        Task<bool> ClearAsync(int customerID);
        Task<int> CountAsync(int customerID);
    }
}