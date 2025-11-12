USE [StockLite]
GO

TRUNCATE TABLE SalesAggregates

SELECT *
FROM SalesAggregates
ORDER BY ProductId, PeriodType

SELECT * FROM Products WHERE Id IS NULL
SELECT * FROM TransactionLineItems

SELECT *
FROM SalesAggregates
WHERE ProductId = '51440746-19b8-4d4f-99cb-d49dbf93800e'

-- Retrieve total quantity and total revenue for a particular product 
-- grouped by day
SELECT 
    DATEPART(Day, TransactionDate) AS [Day]
    ,SUM(L.Quantity)
    ,SUM(L.Quantity * L.UnitPrice)
FROM TransactionLineItems L
INNER JOIN Transactions T
    ON L.TransactionId = T.Id
INNER JOIN Products P
    ON P.Id = L.ProductId
WHERE ProductId = '653fc39b-1cb5-4fae-9a10-0456ffd6d3e3'
AND TransactionDate > DATEADD(MONTH, -1, GETDATE())
GROUP BY DATEPART(DAY, TransactionDate)
ORDER BY [Day]

-- grouped by week
SELECT 
    DATEPART(ISO_WEEK, TransactionDate) AS WEEKOFYEAR
    ,SUM(L.Quantity) AS total_quantity
    ,SUM(L.Quantity * L.UnitPrice) AS total_revenue
FROM TransactionLineItems L
INNER JOIN Transactions T
    ON L.TransactionId = T.Id
INNER JOIN Products P
    ON P.Id = L.ProductId
WHERE ProductId = '653fc39b-1cb5-4fae-9a10-0456ffd6d3e3'
AND TransactionDate > DATEADD(MONTH, -3, GETDATE())
GROUP BY DATEPART(ISO_WEEK, TransactionDate)
ORDER BY WEEKOFYEAR

-- grouped by month
SELECT 
    DATEPART(MONTH, TransactionDate) AS [MONTH]
    ,SUM(L.Quantity) AS total_quantity
    ,SUM(L.Quantity * L.UnitPrice) AS total_revenue
FROM TransactionLineItems L
INNER JOIN Transactions T
    ON L.TransactionId = T.Id
INNER JOIN Products P
    ON P.Id = L.ProductId
WHERE ProductId = '14e1b108-e334-4b8f-a2e7-00365fc5d7eb'
GROUP BY DATEPART(MONTH, TransactionDate)
ORDER BY [MONTH]

-- Get the total amount for a particular transaction
SELECT SUM(P.Price * L.Quantity) AS total_revenue_per_transaction, T.Id
FROM TransactionLineItems L
INNER JOIN Transactions T
    ON L.TransactionId = T.Id
INNER JOIN Products P
    ON P.Id = L.ProductId
WHERE T.Id = 'd8f355fb-ac23-4108-95e2-00d98fb317f7'
GROUP BY T.Id
GO;


/*
BEGIN TRAN
DECLARE @ProductIds dbo.GuidList, @CompanyId UNIQUEIDENTIFIER = 'f9a49ab3-3fa0-4efe-9f40-6629bc742284'

EXEC dbo.InsertSalesAggregates @CompanyId

SELECT * FROM SalesAggregates

ROLLBACK TRAN
*/

/*
BEGIN TRAN
DECLARE @ProductIds dbo.GuidList, @CompanyId UNIQUEIDENTIFIER = 'f9a49ab3-3fa0-4efe-9f40-6629bc742284'

IF NOT EXISTS (SELECT 1 FROM @ProductIds)
BEGIN
    -- Use all products for the company instead
    INSERT INTO @ProductIds (Id)
    SELECT Id FROM dbo.Products WHERE CompanyId = @CompanyId;
END

EXEC dbo.UpdateSalesAggregates @CompanyId, @ProductIds

SELECT *
FROM SalesAggregates
ORDER BY ProductId, PeriodType
ROLLBACK TRAN
GO
*/


BEGIN TRAN
DECLARE @ProductIds dbo.GuidList, @CompanyId UNIQUEIDENTIFIER = 'f9a49ab3-3fa0-4efe-9f40-6629bc742284'

IF NOT EXISTS (SELECT 1 FROM @ProductIds)
BEGIN
    -- Use all products for the company instead
    INSERT INTO @ProductIds (Id)
    SELECT Id FROM dbo.Products WHERE CompanyId = @CompanyId;
END

EXEC dbo.InsertSalesAggregates @CompanyId
EXEC dbo.UpdateSalesAggregates @CompanyId, @ProductIds


ROLLBACK TRAN
GO

