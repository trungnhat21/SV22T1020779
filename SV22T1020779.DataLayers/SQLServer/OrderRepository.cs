using Dapper;
using SV22T1020779.DataLayers.Interfaces;
using SV22T1020779.Models.Common;
using SV22T1020779.Models.Sales;

namespace SV22T1020779.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu liên quan đến đơn hàng và chi tiết đơn hàng trên SQL Server
    /// </summary>
    public class OrderRepository : BaseRepository, IOrderRepository
    {
        /// <summary>
        /// Khởi tạo OrderRepository với chuỗi kết nối CSDL
        /// </summary>
        public OrderRepository(string connectionString) : base(connectionString)
        {
        }

        #region Order Operations

        /// <summary>
        /// Tìm kiếm và lấy danh sách đơn hàng dưới dạng phân trang với các bộ lọc phức hợp
        /// </summary>
        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            using var connection = GetConnection();
            var parameters = new DynamicParameters();

            parameters.Add("@searchValue", $"%{input.SearchValue ?? ""}%");
            parameters.Add("@status", input.Status == 0 ? null : (int?)input.Status);
            parameters.Add("@dateFrom", input.DateFrom);
            parameters.Add("@dateTo", input.DateTo);
            parameters.Add("@pageSize", input.PageSize);
            parameters.Add("@offset", input.Offset);
            parameters.Add("@customerID", input.CustomerID);

            var sql = @"
                SELECT COUNT(*)
                FROM   Orders o
                       LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                WHERE  (@searchValue = N'%%' OR c.CustomerName LIKE @searchValue OR c.Phone LIKE @searchValue)
                  AND  (@status IS NULL OR o.Status = @status)
                  AND  (@dateFrom IS NULL OR o.OrderTime >= @dateFrom)
                  AND  (@dateTo IS NULL OR o.OrderTime <= DATEADD(day, 1, @dateTo))
                  AND  (@customerID = 0 OR o.CustomerID = @customerID);

                SELECT o.*, 
                       ISNULL(c.CustomerName, N'Khách chưa đăng kí') as CustomerName, 
                       ISNULL(c.Phone, N'') as CustomerPhone,
                       e.FullName as EmployeeName,
                       s.ShipperName, s.Phone as ShipperPhone,
                       ISNULL((SELECT SUM(d.Quantity * d.SalePrice) 
                               FROM OrderDetails d 
                               WHERE d.OrderID = o.OrderID), 0) AS TotalPrice
                FROM   Orders o
                       LEFT JOIN Customers c  ON o.CustomerID = c.CustomerID
                       LEFT JOIN Employees e  ON o.EmployeeID = e.EmployeeID
                       LEFT JOIN Shippers s   ON o.ShipperID  = s.ShipperID
                WHERE  (@searchValue = N'%%' OR c.CustomerName LIKE @searchValue OR c.Phone LIKE @searchValue)
                  AND  (@status IS NULL OR o.Status = @status)
                  AND  (@dateFrom IS NULL OR o.OrderTime >= @dateFrom)
                  AND  (@dateTo IS NULL OR o.OrderTime <= DATEADD(day, 1, @dateTo))
                  AND  (@customerID = 0 OR o.CustomerID = @customerID)
                ORDER  BY o.OrderTime DESC
                OFFSET @offset ROWS
                FETCH  NEXT @pageSize ROWS ONLY;";

            using var multi = await connection.QueryMultipleAsync(sql, parameters);
            int rowCount = await multi.ReadFirstAsync<int>();
            var data = (await multi.ReadAsync<OrderViewInfo>()).ToList();

            return new PagedResult<OrderViewInfo>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data
            };
        }

        /// <summary>
        /// Lấy thông tin đầy đủ của một đơn hàng bao gồm thông tin liên kết từ các bảng khác
        /// </summary>
        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            using var connection = GetConnection();
            var sql = @"SELECT o.*,
                               ISNULL(e.FullName, N'') AS EmployeeName,
                               ISNULL(c.CustomerName, N'') AS CustomerName,
                               ISNULL(c.ContactName, N'') AS CustomerContactName,
                               ISNULL(c.Email, N'') AS CustomerEmail,
                               ISNULL(c.Phone, N'') AS CustomerPhone,
                               ISNULL(c.Address, N'') AS CustomerAddress,
                               ISNULL(s.ShipperName, N'') AS ShipperName,
                               ISNULL(s.Phone, N'') AS ShipperPhone
                        FROM   Orders o
                               LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                               LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                               LEFT JOIN Shippers  s ON o.ShipperID  = s.ShipperID
                        WHERE  o.OrderID = @orderID";
            return await connection.QueryFirstOrDefaultAsync<OrderViewInfo>(sql, new { orderID });
        }

        /// <summary>
        /// Thêm mới một đơn hàng và trả về ID vừa tạo
        /// </summary>
        public async Task<int> AddAsync(Order data)
        {
            using var connection = GetConnection();
            var sql = @"INSERT INTO Orders (CustomerID, OrderTime, DeliveryProvince, DeliveryAddress,
                                            EmployeeID, AcceptTime, ShipperID, ShippedTime, FinishedTime, Status)
                        VALUES (@CustomerID, @OrderTime, @DeliveryProvince, @DeliveryAddress,
                                @EmployeeID, @AcceptTime, @ShipperID, @ShippedTime, @FinishedTime, @Status);
                        SELECT SCOPE_IDENTITY();";
            var result = await connection.ExecuteScalarAsync<decimal>(sql, data);
            return (int)result;
        }

        /// <summary>
        /// Cập nhật thông tin hành chính của đơn hàng
        /// </summary>
        public async Task<bool> UpdateAsync(Order data)
        {
            using var connection = GetConnection();
            var sql = @"UPDATE Orders
                        SET CustomerID       = @CustomerID,
                            OrderTime        = @OrderTime,
                            DeliveryProvince = @DeliveryProvince,
                            DeliveryAddress  = @DeliveryAddress,
                            EmployeeID       = @EmployeeID,
                            AcceptTime       = @AcceptTime,
                            ShipperID        = @ShipperID,
                            ShippedTime      = @ShippedTime,
                            FinishedTime     = @FinishedTime,
                            Status           = @Status
                        WHERE OrderID = @OrderID";
            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa đơn hàng và tất cả các chi tiết liên quan (Cascade Delete thủ công)
        /// </summary>
        public async Task<bool> DeleteAsync(int orderID)
        {
            using var connection = GetConnection();
            var sql = @"DELETE FROM OrderDetails WHERE OrderID = @orderID;
                        DELETE FROM Orders WHERE OrderID = @orderID;";
            int rows = await connection.ExecuteAsync(sql, new { orderID });
            return rows > 0;
        }

        #endregion

        #region Order Detail Operations

        /// <summary>
        /// Danh sách các mặt hàng thuộc một đơn hàng cụ thể
        /// </summary>
        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            using var connection = GetConnection();
            var sql = @"SELECT d.*,
                               ISNULL(p.ProductName, N'') AS ProductName,
                               ISNULL(p.Unit, N'') AS Unit,
                               ISNULL(p.Photo, N'') AS Photo
                        FROM   OrderDetails d
                               LEFT JOIN Products p ON d.ProductID = p.ProductID
                        WHERE  d.OrderID = @orderID
                        ORDER  BY p.ProductName";
            var data = await connection.QueryAsync<OrderDetailViewInfo>(sql, new { orderID });
            return data.ToList();
        }

        /// <summary>
        /// Lấy chi tiết của một sản phẩm trong đơn hàng
        /// </summary>
        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            using var connection = GetConnection();
            var sql = @"SELECT d.*,
                               ISNULL(p.ProductName, N'') AS ProductName,
                               ISNULL(p.Unit, N'') AS Unit,
                               ISNULL(p.Photo, N'') AS Photo
                        FROM   OrderDetails d
                               LEFT JOIN Products p ON d.ProductID = p.ProductID
                        WHERE  d.OrderID   = @orderID
                          AND  d.ProductID = @productID";
            return await connection.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(sql, new { orderID, productID });
        }

        /// <summary>
        /// Thêm mới hoặc cập nhật thông tin sản phẩm trong đơn hàng
        /// </summary>
        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            using var connection = GetConnection();
            var sql = @"IF EXISTS (SELECT 1 FROM OrderDetails WHERE OrderID = @OrderID AND ProductID = @ProductID)
                            UPDATE OrderDetails
                            SET    Quantity  = @Quantity,
                                   SalePrice = @SalePrice
                            WHERE  OrderID   = @OrderID
                              AND  ProductID = @ProductID
                        ELSE
                            INSERT INTO OrderDetails (OrderID, ProductID, Quantity, SalePrice)
                            VALUES (@OrderID, @ProductID, @Quantity, @SalePrice)";
            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Cập nhật số lượng hoặc giá của một mặt hàng cụ thể
        /// </summary>
        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            using var connection = GetConnection();
            var sql = @"UPDATE OrderDetails
                        SET Quantity  = @Quantity,
                            SalePrice = @SalePrice
                        WHERE OrderID   = @OrderID
                          AND ProductID = @ProductID";
            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Loại bỏ một mặt hàng khỏi đơn hàng
        /// </summary>
        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using var connection = GetConnection();
            var sql = "DELETE FROM OrderDetails WHERE OrderID = @orderID AND ProductID = @productID";
            int rows = await connection.ExecuteAsync(sql, new { orderID, productID });
            return rows > 0;
        }

        #endregion
    }
}