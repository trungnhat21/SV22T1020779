using SV22T1020779.DataLayers.Interfaces;
using SV22T1020779.DataLayers.SQLServer;
using SV22T1020779.Models.DataDictionary;

namespace SV22T1020779.BusinessLayers
{
    public static class DictionaryDataService
    {
        private static readonly IDataDictionaryRepository<Province> provinceDB;
        /// <summary>
        /// Ctor của 1 lớp static k được có tham số và được gọi khi lớp này đc sử dụng lần đầu tiền, không được có public ở
        /// phía trước
        /// </summary>
        static DictionaryDataService()
        {
            provinceDB = new ProvinceRepository(Configuration.ConnectionString);
        }
        public static async Task<List<Province>> ListProvincesAsync()
        {
            return await provinceDB.ListAsync();
        }
    }
}
