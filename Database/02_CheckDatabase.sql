USE moridb_v2;
GO
SELECT * FROM dbo.Categories ORDER BY CategoryName;
SELECT * FROM dbo.Suppliers ORDER BY SupplierName;
SELECT * FROM dbo.Products ORDER BY ProductName;
SELECT * FROM dbo.Sizes ORDER BY SizeValue;
SELECT * FROM dbo.ProductSizes ORDER BY ProductID, SizeID;
SELECT * FROM dbo.vw_ProductCatalog ORDER BY ProductName;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;
GO
