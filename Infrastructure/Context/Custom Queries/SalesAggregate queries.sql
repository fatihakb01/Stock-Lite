USE [BILite]
GO

TRUNCATE TABLE SalesAggregates

SELECT *
FROM SalesAggregates
ORDER BY ProductId, PeriodType


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
WHERE ProductId = '653fc39b-1cb5-4fae-9a10-0456ffd6d3e3'
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