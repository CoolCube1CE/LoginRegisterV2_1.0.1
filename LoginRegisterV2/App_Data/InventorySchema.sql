

IF OBJECT_ID('dbo.StockItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockItems
    (
        ItemId        INT IDENTITY(1,1) PRIMARY KEY,
        ItemName      NVARCHAR(150)   NOT NULL,
        Category      NVARCHAR(100)   NULL,
        SKU           NVARCHAR(50)    NULL,
        Quantity      INT             NOT NULL DEFAULT (0),
        ReorderLevel  INT             NOT NULL DEFAULT (10),
        UnitPrice     DECIMAL(10,2)   NOT NULL DEFAULT (0),
        CreatedAt     DATETIME        NOT NULL DEFAULT (GETDATE()),
        UpdatedAt     DATETIME        NOT NULL DEFAULT (GETDATE())
    );
END
GO

IF OBJECT_ID('dbo.StockTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockTransactions
    (
        TransactionId    INT IDENTITY(1,1) PRIMARY KEY,
        ItemId           INT             NOT NULL,
        TransactionType  VARCHAR(10)     NOT NULL,  -- 'IN' or 'OUT'
        Quantity         INT             NOT NULL,
        Reference        NVARCHAR(150)   NULL,      -- e.g. "Order #12" or "Manual Restock"
        PerformedBy      NVARCHAR(256)   NULL,      -- email of the logged-in user
        TransactionDate  DATETIME        NOT NULL DEFAULT (GETDATE()),
        Notes            NVARCHAR(300)   NULL,
        CONSTRAINT FK_StockTransactions_StockItems
            FOREIGN KEY (ItemId) REFERENCES dbo.StockItems(ItemId),
        CONSTRAINT CK_StockTransactions_Type
            CHECK (TransactionType IN ('IN', 'OUT'))
    );

    CREATE INDEX IX_StockTransactions_ItemId ON dbo.StockTransactions(ItemId);
    CREATE INDEX IX_StockTransactions_Date ON dbo.StockTransactions(TransactionDate DESC);
END
GO

IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        OrderId       INT IDENTITY(1,1) PRIMARY KEY,
        CustomerName  NVARCHAR(150)   NOT NULL,
        ItemId        INT             NOT NULL,
        Quantity      INT             NOT NULL,
        UnitPrice     DECIMAL(10,2)   NOT NULL,
        TotalAmount   AS (CAST(Quantity AS DECIMAL(10,2)) * UnitPrice) PERSISTED,
        Status        VARCHAR(20)     NOT NULL DEFAULT ('Pending'),  -- Pending / Completed / Cancelled
        OrderDate     DATETIME        NOT NULL DEFAULT (GETDATE()),
        PlacedBy      NVARCHAR(256)   NULL,
        CONSTRAINT FK_Orders_StockItems
            FOREIGN KEY (ItemId) REFERENCES dbo.StockItems(ItemId),
        CONSTRAINT CK_Orders_Status
            CHECK (Status IN ('Pending', 'Completed', 'Cancelled'))
    );

    CREATE INDEX IX_Orders_Date ON dbo.Orders(OrderDate DESC);
END
GO
