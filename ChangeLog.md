## Ch?c n?ng cho 22T1020779.Admin
	- Trang ch?: Home/Index
	- Tài kho?n:
		+ Acount/Login
		+ Acount/Logout
		+ Acount/ChangePassword
	- Supplier:
		+ Suppiler/Index
		+ Suppiler/Create
		+ Suppiler/Edit/{id}
		+ Suppiler/Delete/{id}
	- Customer:
		+ Customer/Index
		+ Customer/Create
		+ Customer/Edit/{id}
		+ Customer/Delete/{id} 
		+ Customer/ChangePassword/{id}
	- Shipper:
		+ Shipper/Index
		+ Shipper/Create
		+ Shipper/Edit/{id}
		+ Shipper/Delete/{id}
	- Employee:
		+ Employee/Index
		+ Employee/Create
		+ Employee/Edit/{id}
		+ Employee/Delete/{id}
		+ Employee/ChangePassword/{id}
		+ Employee/ChangeRoles/{id}
	- Category:
		+ Category/Index
		+ Category/Create
		+ Category/Edit/{id}
		+ Category/Delete/{id}
	- Product:
		+ Product/Index
			*Tìm ki?m, l?c m?t hàng theo nhà cung c?p, phân lo?i, kho?ng giá, tên*
			*Hi?n th? d??i d?ng phân trang*
		+ Product/Create
		+ Product/Edit/{id}
		+ Product/Delete/{id}
		+ Product/Detail/{id}
		+ Product/ListAttributes/{id}
		+ Product/AddAttributes/{id}
		+ Product/EditAttributes/{id}?attributeId={attributeId}
		+ Product/DeleteAttribute/{id}?attributeId={attributeId}
		+ Product/ListPhoto/{id}
		+ Product/AddPhoto/{id}
		+ Product/EditPhoto/{id}?photoId={photoId}
		+ Product/DeletePhoto/{id}?photoId={photoId}
	- Order:
		+ Order/Index
		+ Order/Detail/{id}
		+ Order/Create

	# Trong file Layout:
		- @RenderBody() : Đat taii vi trí mà noi dung cua các trang web se duoc "ghi" vào dó
		- @{
			await Html.RenderPartialAsyc("PartialView");
		}
		ho?c:
		@await Html.PartialAsyc("PartialView")
		Dùng ?? l?y n?i dung c?a m?t partialView (Phan code HTML duocc tách ra o 1 file view) 
		và "ghi/chèn" vào mot ví trí nào dó
		- @await RenderSectionAsyc("SelectionName", required fale)
######
	Domain
		-Data distrinary
		-Partner
			+Supplier
			+Customer
			+Shipper
		-Hr
			+Employee
		-Catalog
			+Category
			+Product
			+Product Attribute
			+Product Photo
		-Sale
			+OrderStatus
			+Order
			+OrderDetail
		-Security
			+User Account
		-Common

	Interfaces dùng để định nghĩa các "giao diện" xử lý dữ liệu
	Viết lớp SupplierResponsitory cài đặt interface trên:
		+Sử dụng Dappper
		+CSDL SQLServer
		
###
	-Tìm kiếm phân trang: Đầu vào tìm kiếm, phân trang: Page, PagwSize, SearchValue (nhà cc, khách hàng, shipper, category, employee)
	-Lấy thông tin của 1 đối tượng dựa vào id
	-Bổ sung 1 đối tượng vào CSDL
	-Cập nhật 1 đối tượng trong CSDL
	-Xóa 1 đối tượng trong CSDL

###
	-Khi Action có trả dữ liệu về cho View thì phải biết kiểu dữ liệu trả về là gì
	-Trong View (trên cùng của view), phải có chỉ thị khai báo kiểu dữ liệu mà Action trả về
		"@model Kiểu_dữ_liệu"
	-Trong View, dữ liệu mà Action trả về lưu trong thuộc tính có tên là Model (trong View thông qua thuộc tính này để lấy dữ liệu)
	-Kiểu dữ liệu là tên lớp (VD: Customer)
	-MUốn gửi dữ liệu lên server phải có name
	-ASP-FOR chỉ sử dụng khi có model 
	
###
	-Người dùng cung cấp thông tin để kiểm tra xem có được phép vào hệ thống hay không?
	-Hệ thống kiểm tra, nếu hợp lệ thì cấp cho một Cookie
	-Phía Client xuất trình Cookie mỗi khi thực hiện các Request (Kèm cookie trong header của lời gọi)
	-Phía server dựa vào cookie để kiểm tra
	**** Thuật ngữ
		-Authentication
		-Authorization (xác thực phân quyền)