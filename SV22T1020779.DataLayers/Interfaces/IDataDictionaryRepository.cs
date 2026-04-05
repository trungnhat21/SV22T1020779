namespace SV22T1020779.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu sử dụng cho từ điển dữ liệu
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataDictionaryRepository<T> where T : class
    {
        Task<List<T>> ListAsync();
    }
}
