

IF DB_ID(N'moridb_v2') IS NULL
BEGIN
    CREATE DATABASE moridb_v2;
END;
GO
USE moridb_v2;
GO

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        CategoryID INT IDENTITY(1,1) CONSTRAINT PK_Categories PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL CONSTRAINT UQ_Categories_CategoryName UNIQUE
    );
END;
GO

IF OBJECT_ID(N'dbo.Suppliers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Suppliers
    (
        SupplierID INT IDENTITY(1,1) CONSTRAINT PK_Suppliers PRIMARY KEY,
        SupplierName NVARCHAR(150) NOT NULL CONSTRAINT UQ_Suppliers_SupplierName UNIQUE,
        Phone NVARCHAR(30) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        ProductID INT IDENTITY(1,1) CONSTRAINT PK_Products PRIMARY KEY,
        Article NVARCHAR(30) NOT NULL CONSTRAINT UQ_Products_Article UNIQUE,
        ProductName NVARCHAR(150) NOT NULL,
        CategoryID INT NOT NULL,
        SupplierID INT NOT NULL,
        Manufacturer NVARCHAR(120) NOT NULL,
        Composition NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        Price DECIMAL(10,2) NOT NULL CONSTRAINT CK_Products_Price CHECK (Price > 0),
        ImagePath NVARCHAR(500) NULL,
        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryID) REFERENCES dbo.Categories(CategoryID),
        CONSTRAINT FK_Products_Suppliers FOREIGN KEY (SupplierID) REFERENCES dbo.Suppliers(SupplierID)
    );
END;
GO

IF OBJECT_ID(N'dbo.Sizes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sizes
    (
        SizeID INT IDENTITY(1,1) CONSTRAINT PK_Sizes PRIMARY KEY,
        SizeValue INT NOT NULL CONSTRAINT UQ_Sizes_SizeValue UNIQUE,
        CONSTRAINT CK_Sizes_SizeValue CHECK (SizeValue BETWEEN 30 AND 50)
    );
END;
GO

IF OBJECT_ID(N'dbo.ProductSizes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductSizes
    (
        ProductID INT NOT NULL,
        SizeID INT NOT NULL,
        Quantity INT NOT NULL CONSTRAINT DF_ProductSizes_Quantity DEFAULT (0),
        CONSTRAINT PK_ProductSizes PRIMARY KEY (ProductID, SizeID),
        CONSTRAINT FK_ProductSizes_Products FOREIGN KEY (ProductID) REFERENCES dbo.Products(ProductID),
        CONSTRAINT FK_ProductSizes_Sizes FOREIGN KEY (SizeID) REFERENCES dbo.Sizes(SizeID),
        CONSTRAINT CK_ProductSizes_Quantity CHECK (Quantity >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers
    (
        CustomerID INT IDENTITY(1,1) CONSTRAINT PK_Customers PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Phone NVARCHAR(30) NULL,
        Email NVARCHAR(120) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserID INT IDENTITY(1,1) CONSTRAINT PK_Users PRIMARY KEY,
        Login NVARCHAR(50) NOT NULL CONSTRAINT UQ_Users_Login UNIQUE,
        CustomerID INT NOT NULL CONSTRAINT UQ_Users_CustomerID UNIQUE,
        CONSTRAINT FK_Users_Customers FOREIGN KEY (CustomerID) REFERENCES dbo.Customers(CustomerID)
    );
END;
GO



IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        OrderID INT IDENTITY(1,1) CONSTRAINT PK_Orders PRIMARY KEY,
        CustomerID INT NOT NULL,
        OrderDate DATETIME2 NOT NULL CONSTRAINT DF_Orders_OrderDate DEFAULT (SYSDATETIME()),
        Status NVARCHAR(40) NOT NULL CONSTRAINT DF_Orders_Status DEFAULT (N'Создан'),
        CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerID) REFERENCES dbo.Customers(CustomerID)
    );
END;
GO

IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems
    (
        OrderItemID INT IDENTITY(1,1) CONSTRAINT PK_OrderItems PRIMARY KEY,
        OrderID INT NOT NULL,
        ProductID INT NOT NULL,
        Quantity INT NOT NULL CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0),
        UnitPrice DECIMAL(10,2) NOT NULL CONSTRAINT CK_OrderItems_UnitPrice CHECK (UnitPrice > 0),
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderID) REFERENCES dbo.Orders(OrderID) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductID) REFERENCES dbo.Products(ProductID)
    );
END;
GO


IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    INSERT dbo.Categories(CategoryName)
    VALUES (N'Спортивная обувь'), (N'Классическая обувь'), (N'Зимняя обувь');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Suppliers)
BEGIN
    INSERT dbo.Suppliers(SupplierName, Phone)
    VALUES
        (N'ООО СпортОбувь', N'+7 3012 11-22-33'),
        (N'ООО Модная обувь', N'+7 3012 22-33-44'),
        (N'ООО Комфорт', N'+7 3012 33-44-55');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Sizes)
BEGIN
    INSERT dbo.Sizes(SizeValue) VALUES (38),(39),(40),(41),(42),(43),(44),(45);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    INSERT dbo.Products
        (Article, ProductName, CategoryID, SupplierID, Manufacturer, Composition, Description, Price, ImagePath)
    SELECT v.Article, v.ProductName, c.CategoryID, s.SupplierID,
           v.Manufacturer, v.Composition, v.Description, v.Price, NULL
    FROM (VALUES
        (N'SP-014', N'Кроссовки Sprint', N'Спортивная обувь', N'ООО СпортОбувь', N'Фабрика Вектор', N'Текстиль и искусственная кожа', N'Лёгкие повседневные кроссовки', CAST(4290.00 AS DECIMAL(10,2))),
        (N'ST-027', N'Кеды Street', N'Спортивная обувь', N'ООО СпортОбувь', N'Мастерская Город', N'Хлопок и резина', N'Текстильные кеды на каждый день', CAST(2790.00 AS DECIMAL(10,2))),
        (N'CL-031', N'Туфли Classic', N'Классическая обувь', N'ООО Модная обувь', N'Фабрика Стиль', N'Натуральная кожа', N'Классические туфли', CAST(5990.00 AS DECIMAL(10,2))),
        (N'WN-042', N'Ботинки North', N'Зимняя обувь', N'ООО Комфорт', N'Фабрика Север', N'Натуральная кожа и утеплитель', N'Зимние утеплённые ботинки', CAST(7490.00 AS DECIMAL(10,2))),
        (N'WN-056', N'Ботинки Urban', N'Зимняя обувь', N'ООО Комфорт', N'Производство Альянс', N'Искусственная кожа и текстиль', N'Городские ботинки', CAST(6290.00 AS DECIMAL(10,2)))
    ) AS v(Article, ProductName, CategoryName, SupplierName, Manufacturer, Composition, Description, Price)
    INNER JOIN dbo.Categories c ON c.CategoryName = v.CategoryName
    INNER JOIN dbo.Suppliers s ON s.SupplierName = v.SupplierName;
END;
GO



UPDATE dbo.Products SET ImagePath = N'Resources/products/sprint.png' WHERE Article = N'SP-014';
UPDATE dbo.Products SET ImagePath = N'Resources/products/street.png' WHERE Article = N'ST-027';
UPDATE dbo.Products SET ImagePath = N'Resources/products/classic.png' WHERE Article = N'CL-031';
UPDATE dbo.Products SET ImagePath = N'Resources/products/north.png' WHERE Article = N'WN-042';
UPDATE dbo.Products SET ImagePath = N'Resources/products/urban.png' WHERE Article = N'WN-056';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ProductSizes)
BEGIN
    INSERT dbo.ProductSizes(ProductID, SizeID, Quantity)
    SELECT p.ProductID, z.SizeID, v.Quantity
    FROM (VALUES
        (N'SP-014', 40, 4), (N'SP-014', 41, 6), (N'SP-014', 42, 5), (N'SP-014', 43, 3), (N'SP-014', 44, 2),
        (N'ST-027', 38, 1), (N'ST-027', 39, 1), (N'ST-027', 40, 1),
        (N'CL-031', 39, 3), (N'CL-031', 40, 4), (N'CL-031', 41, 3), (N'CL-031', 42, 2),
        (N'WN-042', 40, 6), (N'WN-042', 41, 5), (N'WN-042', 42, 4), (N'WN-042', 43, 2),
        (N'WN-056', 40, 1), (N'WN-056', 41, 1), (N'WN-056', 42, 0)
    ) AS v(Article, SizeValue, Quantity)
    INNER JOIN dbo.Products p ON p.Article = v.Article
    INNER JOIN dbo.Sizes z ON z.SizeValue = v.SizeValue;
END;
GO



IF NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE FullName = N'Демо Покупатель')
    INSERT dbo.Customers(FullName, Phone) VALUES (N'Демо Покупатель', N'+7 900 000-00-00');
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE Status = N'Демонстрационный')
BEGIN
    DECLARE @DemoCustomerID INT;
    DECLARE @DemoProductID INT;
    DECLARE @DemoOrderID INT;

    SELECT TOP (1) @DemoCustomerID = CustomerID
    FROM dbo.Customers WHERE FullName = N'Демо Покупатель' ORDER BY CustomerID;

    SELECT @DemoProductID = ProductID FROM dbo.Products WHERE Article = N'SP-014';

    INSERT dbo.Orders(CustomerID, OrderDate, Status)
    VALUES (@DemoCustomerID, DATEADD(DAY, -1, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)), N'Демонстрационный');

    SET @DemoOrderID = SCOPE_IDENTITY();

    INSERT dbo.OrderItems(OrderID, ProductID, Quantity, UnitPrice)
    VALUES (@DemoOrderID, @DemoProductID, 1, 4290.00);
END;
GO

CREATE OR ALTER VIEW dbo.vw_ProductCatalog
AS
WITH StockByProduct AS
(
    SELECT ProductID, SUM(Quantity) AS TotalQuantity
    FROM dbo.ProductSizes
    GROUP BY ProductID
)
SELECT
    p.ProductID,
    p.Article,
    p.ProductName,
    c.CategoryName,
    p.Manufacturer,
    p.Composition,
    p.Description,
    p.Price,
    CAST(
        CASE WHEN EXISTS
        (
            SELECT 1
            FROM dbo.OrderItems oi
            INNER JOIN dbo.Orders o ON o.OrderID = oi.OrderID
            WHERE oi.ProductID = p.ProductID
              AND o.OrderDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) - 1, 0)
              AND o.OrderDate <  DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
        )
        THEN p.Price
        ELSE p.Price * 0.75
        END AS DECIMAL(10,2)
    ) AS DiscountPrice,
    ISNULL(st.TotalQuantity, 0) AS TotalQuantity,
    CASE WHEN ISNULL(st.TotalQuantity, 0) > 5 THEN N'Много' ELSE N'Мало' END AS QuantityLabel,
    CAST(CASE WHEN ISNULL(st.TotalQuantity, 0) <= 3 THEN 1 ELSE 0 END AS BIT) AS LowStock,
    p.ImagePath
FROM dbo.Products p
INNER JOIN dbo.Categories c ON c.CategoryID = p.CategoryID
LEFT JOIN StockByProduct st ON st.ProductID = p.ProductID;
GO

SELECT N'База данных moridb_v2, таблицы, тестовые данные и каталог созданы.' AS Result;
GO
