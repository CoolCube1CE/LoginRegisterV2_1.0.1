
-- ---------- 1. Staff accounts (table: admin) ----------
IF COL_LENGTH('dbo.admin', 'username') IS NULL
    ALTER TABLE dbo.admin ADD username NVARCHAR(50) NULL;
GO

;WITH names AS (
    SELECT email,
           LOWER(LEFT(email, CHARINDEX('@', email + '@') - 1)) AS base,
           ROW_NUMBER() OVER (ORDER BY email) AS rn
    FROM dbo.admin
    WHERE username IS NULL OR username = ''
)
UPDATE a
SET a.username = CASE
        WHEN (SELECT COUNT(*) FROM names n2 WHERE n2.base = n.base) = 1
             AND NOT EXISTS (SELECT 1 FROM dbo.admin x WHERE x.username = n.base)
        THEN LEFT(n.base, 50)
        ELSE LEFT(n.base, 40) + CAST(n.rn AS VARCHAR(9))
    END
FROM dbo.admin a
INNER JOIN names n ON n.email = a.email;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_admin_username' AND object_id = OBJECT_ID('dbo.admin'))
    CREATE UNIQUE INDEX UX_admin_username ON dbo.admin(username) WHERE username IS NOT NULL;
GO

UPDATE dbo.admin SET accesstype = 'Admin' WHERE accesstype = 'Super Admin';
GO

-- ---------- 2. Beverages (table: StockItems) ----------
IF COL_LENGTH('dbo.StockItems', 'ImagePath') IS NULL
    ALTER TABLE dbo.StockItems ADD ImagePath NVARCHAR(260) NULL;
GO

IF COL_LENGTH('dbo.StockItems', 'IsActive') IS NULL
    ALTER TABLE dbo.StockItems ADD IsActive BIT NOT NULL CONSTRAINT DF_StockItems_IsActive DEFAULT (1);
GO

-- OPTIONAL
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_StockItems_Quantity_NonNegative')
    ALTER TABLE dbo.StockItems WITH NOCHECK ADD CONSTRAINT CK_StockItems_Quantity_NonNegative CHECK (Quantity >= 0);
GO

-- ---------- 3. Stock batches (FIFO) ----------
IF OBJECT_ID('dbo.StockBatches', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockBatches
    (
        BatchId            INT IDENTITY(1,1) PRIMARY KEY,
        ItemId             INT            NOT NULL,
        QuantityReceived   INT            NOT NULL,
        QuantityRemaining  INT            NOT NULL,
        ReceivedDate       DATETIME       NOT NULL CONSTRAINT DF_StockBatches_Received DEFAULT (GETDATE()),
        Notes              NVARCHAR(300)  NULL,
        ReceivedBy         NVARCHAR(256)  NULL,
        CONSTRAINT FK_StockBatches_StockItems FOREIGN KEY (ItemId) REFERENCES dbo.StockItems(ItemId),
        CONSTRAINT CK_StockBatches_Qty CHECK (QuantityReceived > 0 AND QuantityRemaining >= 0 AND QuantityRemaining <= QuantityReceived)
    );

    CREATE INDEX IX_StockBatches_Fifo ON dbo.StockBatches(ItemId, ReceivedDate, BatchId) INCLUDE (QuantityRemaining);
END
GO

-- Existing stock becomes one "opening" batch per beverage (only if it has no batch yet)
INSERT INTO dbo.StockBatches (ItemId, QuantityReceived, QuantityRemaining, ReceivedDate, Notes, ReceivedBy)
SELECT i.ItemId, i.Quantity, i.Quantity, i.CreatedAt, N'Opening stock (carried over)', NULL
FROM dbo.StockItems i
WHERE i.Quantity > 0
  AND NOT EXISTS (SELECT 1 FROM dbo.StockBatches b WHERE b.ItemId = i.ItemId);
GO

-- ---------- 4. Orders with many items ----------
IF COL_LENGTH('dbo.Orders', 'GrandTotal') IS NULL
    ALTER TABLE dbo.Orders ADD GrandTotal DECIMAL(12,2) NULL;
GO

IF COL_LENGTH('dbo.Orders', 'CompletedAt') IS NULL
    ALTER TABLE dbo.Orders ADD CompletedAt DATETIME NULL;
GO

-- The old computed TotalAmount only worked for one item per order: keep its values in GrandTotal, then remove it
IF EXISTS (SELECT 1 FROM sys.computed_columns WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'TotalAmount')
BEGIN
    EXEC(N'UPDATE dbo.Orders SET GrandTotal = TotalAmount WHERE GrandTotal IS NULL');
    ALTER TABLE dbo.Orders DROP COLUMN TotalAmount;
END
GO

IF COL_LENGTH('dbo.Orders', 'Quantity') IS NOT NULL
    EXEC(N'UPDATE dbo.Orders SET GrandTotal = ISNULL(Quantity, 0) * ISNULL(UnitPrice, 0) WHERE GrandTotal IS NULL');
GO

UPDATE dbo.Orders SET GrandTotal = 0 WHERE GrandTotal IS NULL;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'GrandTotal' AND is_nullable = 1)
    ALTER TABLE dbo.Orders ALTER COLUMN GrandTotal DECIMAL(12,2) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Orders_GrandTotal')
    ALTER TABLE dbo.Orders ADD CONSTRAINT DF_Orders_GrandTotal DEFAULT (0) FOR GrandTotal;
GO

-- The single-item columns of the old design are no longer required
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'ItemId' AND is_nullable = 0)
    ALTER TABLE dbo.Orders ALTER COLUMN ItemId INT NULL;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'Quantity' AND is_nullable = 0)
    ALTER TABLE dbo.Orders ALTER COLUMN Quantity INT NULL;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'UnitPrice' AND is_nullable = 0)
    ALTER TABLE dbo.Orders ALTER COLUMN UnitPrice DECIMAL(10,2) NULL;
GO

UPDATE dbo.Orders SET CompletedAt = OrderDate WHERE Status = 'Completed' AND CompletedAt IS NULL;
GO

IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems
    (
        OrderItemId  INT IDENTITY(1,1) PRIMARY KEY,
        OrderId      INT            NOT NULL,
        ItemId       INT            NOT NULL,
        Quantity     INT            NOT NULL,
        UnitPrice    DECIMAL(10,2)  NOT NULL,
        LineTotal    AS (CAST(Quantity AS DECIMAL(12,2)) * UnitPrice) PERSISTED,
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(OrderId),
        CONSTRAINT FK_OrderItems_StockItems FOREIGN KEY (ItemId) REFERENCES dbo.StockItems(ItemId),
        CONSTRAINT CK_OrderItems_Qty CHECK (Quantity > 0)
    );

    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
    CREATE INDEX IX_OrderItems_ItemId ON dbo.OrderItems(ItemId);
END
GO

-- Orders created by the old one-item screen become one order line each (only once)
IF COL_LENGTH('dbo.Orders', 'ItemId') IS NOT NULL
    EXEC(N'INSERT INTO dbo.OrderItems (OrderId, ItemId, Quantity, UnitPrice)
           SELECT o.OrderId, o.ItemId, o.Quantity, o.UnitPrice
           FROM dbo.Orders o
           WHERE o.ItemId IS NOT NULL AND o.Quantity IS NOT NULL AND o.UnitPrice IS NOT NULL
             AND NOT EXISTS (SELECT 1 FROM dbo.OrderItems oi WHERE oi.OrderId = o.OrderId)');
GO

IF OBJECT_ID('dbo.OrderItemBatches', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItemBatches
    (
        AllocationId  INT IDENTITY(1,1) PRIMARY KEY,
        OrderItemId   INT NOT NULL,
        BatchId       INT NOT NULL,
        Quantity      INT NOT NULL,
        CONSTRAINT FK_OrderItemBatches_OrderItems FOREIGN KEY (OrderItemId) REFERENCES dbo.OrderItems(OrderItemId),
        CONSTRAINT FK_OrderItemBatches_StockBatches FOREIGN KEY (BatchId) REFERENCES dbo.StockBatches(BatchId),
        CONSTRAINT CK_OrderItemBatches_Qty CHECK (Quantity > 0)
    );

    CREATE INDEX IX_OrderItemBatches_OrderItemId ON dbo.OrderItemBatches(OrderItemId);
END
GO

-- OPTIONAL
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Status_Completed' AND object_id = OBJECT_ID('dbo.Orders'))
    CREATE INDEX IX_Orders_Status_Completed ON dbo.Orders(Status, CompletedAt);
GO
