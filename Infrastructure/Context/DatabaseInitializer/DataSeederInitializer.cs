using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer;

public static class DataSeederInitializer
{
    public static async Task InitializeAsync(DataContext context, ILogger logger)
    {
		await DailyTransactionRecords(context, logger);
		await DailyPoRecords(context, logger);
		await DailyCoRecords(context, logger); 
		await DailyProductRecords(context, logger);

        await CreateCompanyProcAsync(context, logger);
        await CleanMasterData(context, logger);
		await CreateFulfilledPOs(context, logger);
        await SeedData(context, logger);
    }

    private static async Task DailyTransactionRecords(DataContext context, ILogger logger)
	{
        logger.LogInformation("Ensuring dbo.DailyTransactionRecords stored procedures exist...");


		var sql = @"
		CREATE OR ALTER PROCEDURE [dbo].[DailyTransactionRecords] 
		@CompanyId UNIQUEIDENTIFIER,
		@NumberOfTransactions INT
		AS
		BEGIN

			DECLARE @TransactionCount INT

			SELECT @TransactionCount = COUNT(*) FROM Transactions WHERE CompanyId = @CompanyId;

			SELECT TOP (@NumberOfTransactions) *
			FROM (
				-- New Transactions
				SELECT TOP (@NumberOfTransactions * 9 / 10)
					CONCAT('T-', FORMAT(1 + @TransactionCount + ROW_NUMBER() OVER (ORDER BY p.Id), 'D8')) AS TransactionId,
					p.SKU AS SKU,
					DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 30, SYSUTCDATETIME()) AS TransactionDate,
					CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN 'Card' ELSE 'Cash' END AS PaymentMethod,
					NULL AS Notes,
					ABS(CHECKSUM(NEWID())) % 46 + 5 AS Quantity
				FROM Products p

				UNION ALL

				-- Existing Transactions
				SELECT TOP (@NumberOfTransactions / 10)
					t.TransactionId,
					p.SKU,
					t.TransactionDate,
					t.PaymentMethod,
					t.Notes,
					tl.Quantity
				FROM Transactions t
				INNER JOIN TransactionLineItems tl ON tl.TransactionId = t.Id
				INNER JOIN Products p ON p.Id = tl.ProductId
			) x
			ORDER BY NEWID();
		END
		";

        await context.Database.ExecuteSqlRawAsync(sql);
	}

    private static async Task DailyPoRecords(DataContext context, ILogger logger)
	{
        logger.LogInformation("Ensuring dbo.DailyPoRecords stored procedures exist...");

		var sql = @"
		CREATE OR ALTER PROCEDURE [dbo].[DailyPoRecords] 
		@CompanyId UNIQUEIDENTIFIER,
		@NumberOfPOs INT
		AS
		BEGIN

			DECLARE @PoCount INT

			SELECT @PoCount = COUNT(*) FROM PurchaseOrder WHERE CompanyId = @CompanyId;

			SELECT TOP (@NumberOfPOs) *
			FROM (
				-- New POs
				SELECT TOP (@NumberOfPOs * 9 / 10)
					CONCAT('PO-', FORMAT(1 + @PoCount + ROW_NUMBER() OVER (ORDER BY p.Id), 'D8')) AS OrderNumber,
					p.SKU AS SKU,
					DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 10, SYSUTCDATETIME()) AS OrderDate,
					DATEADD(DAY, ABS(CHECKSUM(NEWID())) % 30, SYSUTCDATETIME()) AS ExpectedDeliveryDate,
					NULL AS ReceivedDate,
					ABS(CHECKSUM(NEWID())) % 46 + 5 AS Quantity,
					0 AS ReceivedQuantity
				FROM Products p

				UNION ALL

				-- Existing POs
				SELECT TOP (@NumberOfPOs / 10)
					po.OrderNumber,
					p.SKU,
					po.OrderDate,
					po.ExpectedDeliveryDate,
					po.ReceivedDate,
					pl.Quantity,
					ISNULL(pl.ReceivedQuantity,0)
				FROM PurchaseOrder po
				INNER JOIN PurchaseOrderLineItem pl ON pl.PurchaseOrderId = po.Id
				INNER JOIN Products p ON p.Id = pl.ProductId
			) x
			ORDER BY NEWID();

		END
		";

        await context.Database.ExecuteSqlRawAsync(sql);
	}

    private static async Task DailyCoRecords(DataContext context, ILogger logger)
	{
        logger.LogInformation("Ensuring dbo.DailyCoRecords stored procedures exist...");

		var sql = @"
		CREATE OR ALTER PROCEDURE [dbo].[DailyCoRecords] 
		@CompanyId UNIQUEIDENTIFIER,
		@NumberOfCOs INT
		AS
		BEGIN

			DECLARE @CoCount INT

			SELECT @CoCount = COUNT(*) FROM ConfirmedOrder WHERE CompanyId = @CompanyId;

			SELECT TOP (@NumberOfCOs) *
			FROM (
				-- New COs
				SELECT TOP (@NumberOfCOs * 9 / 10)
					CONCAT('CO-', FORMAT(1 + @CoCount + ROW_NUMBER() OVER (ORDER BY p.Id), 'D8')) AS OrderNumber,
					p.SKU AS SKU,
					DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 10, SYSUTCDATETIME()) AS OrderDate,
					DATEADD(DAY, ABS(CHECKSUM(NEWID())) % 30, SYSUTCDATETIME()) AS ExpectedDeliveryDate,
					NULL AS ReceivedDate,
					ABS(CHECKSUM(NEWID())) % 46 + 5 AS Quantity,
					0 AS SoldQuantity
				FROM Products p

				UNION ALL

				-- Existing COs
				SELECT TOP (@NumberOfCOs / 10)
					co.OrderNumber,
					p.SKU,
					co.OrderDate,
					co.ExpectedDeliveryDate,
					ReceivedDate,
					cl.Quantity,
					ISNULL(cl.SoldQuantity,0)
				FROM ConfirmedOrder co
				INNER JOIN ConfirmedOrderLineItem cl ON cl.ConfirmedOrderId = co.Id
				INNER JOIN Products p ON p.Id = cl.ProductId
			) x
			ORDER BY NEWID();

		END
		";

        await context.Database.ExecuteSqlRawAsync(sql);
	}

    private static async Task DailyProductRecords(DataContext context, ILogger logger)
	{
        logger.LogInformation("Ensuring dbo.DailyProductRecords stored procedures exist...");

		var sql = @"
		CREATE OR ALTER PROCEDURE [dbo].[DailyProductRecords] 
		@CompanyId UNIQUEIDENTIFIER,
		@NumberOfProducts INT
		AS
		BEGIN

			DECLARE @ProductCount INT

			SELECT @ProductCount = COUNT(*) FROM Products WHERE CompanyId = @CompanyId;

            -- Define Numbers table
            WITH N AS (
                SELECT TOP (@ProductCount)
                    1 + @ProductCount + ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
                FROM sys.all_objects a
                CROSS JOIN sys.all_objects b
            )
            SELECT * 
            INTO #Numbers
            FROM N;

			SELECT TOP (@NumberOfProducts) *
			FROM (
				-- New Products
				SELECT TOP (@NumberOfProducts * 9 / 10)
					CONCAT('PRD-', FORMAT(n + 1, 'D6')) AS SKU,
					CONCAT('Product ', n) AS Name,
					ROUND(ABS(CHECKSUM(NEWID())) % 500 + 5, 2) AS Price,
					CONCAT('Auto-generated product ', n) AS Description,
					CASE WHEN n % 10 = 0 THEN 0 ELSE 1 END AS isActive,
					GREATEST(ABS(CHECKSUM(NEWID())) % 20, 1) AS LeadTimeDays,
					GREATEST(ABS(CHECKSUM(NEWID())) % 20, 1) AS MinimumOrderQuantity,
					1 AS OrderIncrementQuantity,
					GREATEST(ABS(CHECKSUM(NEWID())) % 10, 1) AS SafetyStock,
					0 AS StockAdjustment
				FROM #Numbers

				UNION ALL

				-- Existing Products
				SELECT TOP (@NumberOfProducts / 10)
					p.SKU,
					p.Name,
					p.Price,
					p.Description,
					p.IsActive,
					pip.LeadTimeDays,
					pip.MinimumOrderQuantity,
					pip.OrderIncrementQuantity,
					pip.SafetyStock,
					pip.StockAdjustment
				FROM Products p
				INNER JOIN ProductInventoryPolicies pip ON p.Id = pip.ProductId
			) x
			ORDER BY NEWID()

		END
		";

        await context.Database.ExecuteSqlRawAsync(sql);
	}

    private static async Task CreateCompanyProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.CreateCompanyProcAsync stored procedures exist...");

        var sql = @"
		/*
		DECLARE @name NVARCHAR(100) = 'Test Company',
				@email NVARCHAR(255) = 'test@test.com',
				@userName NVARCHAR(50) = 'Vatos01',
				@isActive BIT = 1,
				@phoneNumber NVARCHAR(20) = '+31612345678',
				@address NVARCHAR(250) = 'Test street 123'

		EXEC [dbo].[CreateCompany]
			@name,
			@email,
			@isActive,
			@phoneNumber,
			@address
		*/
        CREATE OR ALTER PROCEDURE [dbo].[CreateCompany]
            @name NVARCHAR(100),
            @email NVARCHAR(255),
			@userName NVARCHAR(50),
            @isActive BIT = 1,
            @phoneNumber NVARCHAR(20) = NULL,
            @address NVARCHAR(250) = NULL
        AS
        BEGIN

			DECLARE 
				@CompanyId UNIQUEIDENTIFIER  = NEWID(),
				@UserId UNIQUEIDENTIFIER = NEWID(),
				@JobTaskOne UNIQUEIDENTIFIER = NEWID(),
				@JobTaskTwo UNIQUEIDENTIFIER = NEWID(),
				@CreatedAt DATETIME = GETDATE()

			-- Create new company
            INSERT INTO Companies (
                Id,
                Name,
                Email,
                PhoneNumber,
                Address,
                CreatedAt,
                IsActive
            )
            SELECT
                @CompanyId,
                @name,
                @email,
                @phoneNumber,
                @address,
                GETDATE(),
                @isActive

			-- Create one SuperAdmin user
			INSERT INTO Users (
				Id,
				Role,
				CreatedAt,
				IsActive,
				CompanyId,
				UserName,
				NormalizedUserName,
				Email,
				NormalizedEmail,
				EmailConfirmed,
				PasswordHash,
				SecurityStamp,
				ConcurrencyStamp,
				AccessFailedCount,
				PhoneNumberConfirmed,
				TwoFactorEnabled,
				LockoutEnabled
			)
			SELECT 
				@UserId,
				0,
				GETDATE(),
				1,
				@CompanyId,
				@userName,
				UPPER(@userName),
				@email,
				UPPER(@email),
				1,
				'AQAAAAIAAYagAAAAEJepqc6q+vumDVyE1DCRn0lP0u2RxfsNZyiWZVmv6ta9es9WgTYZSZdUa7KUteFbzQ==', --Password123!
				NEWID(),
				NEWID(),
				0,
				0,
				0,
				0

			-- Create JobTasks
			INSERT INTO JobTasks (
				Id,
				JobTaskKey,
				Name,
				Description,
				AllowManualTrigger,
				IsArchived,
				CompanyId
			)
			VALUES 
			(@JobTaskOne,'JT-1','Import Job Task','',1,0,@CompanyId),
			(@JobTaskTwo,'JT-2','Calculation Jobs','',1,0,@CompanyId)

			-- Create ScheduledJobs
			INSERT INTO ScheduledJobs (
				Id,
				Name,
				Description,
				JobType,
				CalculationType,
				ImportDataType,
				PeriodType,
				Source,
				Target,
				AllowManualTrigger,
				IsArchived,
				JobTaskId,
				ExecutionOrder,
				CreatedAt,
				CreatedBy,
				Result
			)
			VALUES
			(NEWID(),'Import User Defined Fields','','0',NULL,'4',NULL,'C:\CSharp\Data\Master\udf.csv',NULL,'1','0',@JobTaskOne,'4',@CreatedAt,@UserId,'2'),
			(NEWID(),'Import Confirmed Orders','','0',NULL,'2',NULL,'C:\CSharp\Data\Master\confirmedorders.csv',NULL,'1','0',@JobTaskOne,'3',@CreatedAt,@UserId,'2'),
			(NEWID(),'Import Purchase Orders','','0',NULL,'3',NULL,'C:\CSharp\Data\Master\purchaseorders.csv',NULL,'1','0',@JobTaskOne,'2',@CreatedAt,@UserId,'2'),
			(NEWID(),'Import Transactions','','0',NULL,'1',NULL,'C:\CSharp\Data\Master\transactions.csv',NULL,'1','0',@JobTaskOne,'1',@CreatedAt,@UserId,'2'),
			(NEWID(),'Import Products','','0',NULL,'0',NULL,'C:\CSharp\Data\Master\products.csv',NULL,'1','0',@JobTaskOne,'0',@CreatedAt,@UserId,'2'),
			(NEWID(),'Calculate purchase forecast','','1','4',NULL,NULL,NULL,NULL,'1','0',@JobTaskTwo,'4',@CreatedAt,@UserId,'2'),
			(NEWID(),'Calculate confirmed demand','','1','1',NULL,NULL,NULL,NULL,'1','0',@JobTaskTwo,'3',@CreatedAt,@UserId,'2'),
			(NEWID(),'Calculate forecast','','1','2',NULL,NULL,NULL,NULL,'1','0',@JobTaskTwo,'2',@CreatedAt,@UserId,'2'),
			(NEWID(),'Calculate stock levels','','1','0',NULL,NULL,NULL,NULL,'1','0',@JobTaskTwo,'5',@CreatedAt,@UserId,'2'),
			(NEWID(),'Calculate sales','','1','3',NULL,NULL,NULL,NULL,'1','0',@JobTaskTwo,'0',@CreatedAt,@UserId,'2'),
			(NEWID(),'Calculate product KPIs','','1','5',NULL,NULL,NULL,NULL,'1','0',@JobTaskTwo,'6',@CreatedAt,@UserId,'2'),
			(NEWID(),'Calculate seasonality','','1','6',NULL,NULL,NULL,NULL,'1','0',@JobTaskTwo,'1',@CreatedAt,@UserId, '2')
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task CleanMasterData(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.CleanMasterData stored procedures exist...");

        var sql = @"
        CREATE OR ALTER PROCEDURE [dbo].[CleanMasterData]
        AS
        BEGIN

            -- Delete transactions
            DELETE FROM TransactionLineItems;
            DELETE FROM Transactions;

            -- Delete COs
            DELETE FROM ConfirmedOrderLineItem;
            DELETE FROM ConfirmedOrder;

            -- Delete POs
            DELETE FROM PurchaseOrderLineItem;
            DELETE FROM PurchaseOrder;

            -- Delete Products
            DELETE FROM Products;

        END
        ";
        
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task CreateFulfilledPOs(DataContext context, ILogger logger)
	{
        logger.LogInformation("Ensuring dbo.CreateFulfilledPOs stored procedures exist...");

		var sql = @"
		CREATE OR ALTER PROCEDURE [dbo].[CreateFulfilledPOs]
		@CompanyId UNIQUEIDENTIFIER,
		@SafetyBuffer INT = 50,
		@NumberOfProducts INT
		AS
		BEGIN

			-- Return if number of products is unknown
			IF @NumberOfProducts IS NULL
			BEGIN
				SELECT 'Please specify the number of products'
				RETURN
			END

			-- Create POs
			SELECT
				p.ProductId AS ProductId,
				NEWID() AS PurchaseOrderId,
				CONCAT('PO-Dummy-', FORMAT(ROW_NUMBER() OVER (ORDER BY p.ProductId), 'D7')) AS OrderNumber,
				DATEADD(DAY, -1, SYSUTCDATETIME()) AS OrderDate,
				SYSUTCDATETIME() AS ExpectedDeliveryDate,
				SYSUTCDATETIME() AS ReceivedDate,
				QuantityOnHand AS Deficit
			INTO #PO_Source
			FROM dbo.InventorySnapshot p
			WHERE QuantityOnHand <= 0

			INSERT INTO PurchaseOrder (
				Id,
				OrderNumber,
				[Status],
				OrderDate,
				ExpectedDeliveryDate,
				ReceivedDate,
				CompanyId
			)
			SELECT
				PurchaseOrderId,
				OrderNumber,
				3,
				OrderDate,
				ExpectedDeliveryDate,
				ReceivedDate,
				@CompanyId
			FROM #PO_Source

			INSERT INTO PurchaseOrderLineItem (
				PurchaseOrderId,
				ProductId,
				Quantity,
				ReceivedQuantity,
				UnitCost
			)
			SELECT
				PurchaseOrderId,
				p.Id,
				ABS(s.Deficit) + @SafetyBuffer AS Quantity,
				ABS(s.Deficit) + @SafetyBuffer AS ReceivedQuantity,
				p.Price
			FROM #PO_Source s
			JOIN Products p ON p.Id = s.ProductId
			JOIN PurchaseOrder po on po.Id = s.PurchaseOrderId

			DROP TABLE #PO_Source;
		END
		";

        await context.Database.ExecuteSqlRawAsync(sql);
	}

    private static async Task SeedData(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.SeedData stored procedures exist...");

        var sql = @"
        CREATE OR ALTER PROCEDURE [dbo].[SeedData]
        @CompanyId UNIQUEIDENTIFIER,
		@EnsurePositiveStock BIT = 1,
        @NumberOfProducts INT = 100000,
        @NumberOfPOs INT = 75,
        @NumberOfCOs INT = 25,
        @NumberOfTransactions INT = 50
        AS
        BEGIN

            DECLARE @MaxNumber INT;
            
            -- Clear master data
            EXEC [dbo].[CleanMasterData];
            
            -- Define Numbers table
            WITH N AS (
                SELECT TOP (@NumberOfProducts)
                    ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
                FROM sys.all_objects a
                CROSS JOIN sys.all_objects b
            )
            SELECT * 
            INTO #Numbers
            FROM N;
            
            -- Create products
            INSERT INTO Products (
                Id,
                SKU,
                Name,
                Description,
                Price,
                IsActive,
                CreatedAt,
                CompanyId
            )
            SELECT
                NEWID(),
                CONCAT('PRD-', FORMAT(n + 1, 'D6')),
                CONCAT('Product ', n),
                CONCAT('Auto-generated product ', n),
                ROUND(ABS(CHECKSUM(NEWID())) % 500 + 5, 2),
                CASE WHEN n % 10 = 0 THEN 0 ELSE 1 END,
                DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 1000, SYSUTCDATETIME()),
                @CompanyId
            FROM #Numbers
            WHERE n < @NumberOfProducts;

			INSERT INTO ProductInventoryPolicies (
				ProductId,
				LeadTimeDays,
				MinimumOrderQuantity,
				OrderIncrementQuantity,
				SafetyStock,
				StockAdjustment
			)
			SELECT 
				Id,
				GREATEST(ABS(CHECKSUM(Id)) % 20, 1),
				GREATEST(ABS(CHECKSUM(Id)) % 20, 1),
				1,
				GREATEST(ABS(CHECKSUM(Id)) % 10, 1),
				0
			FROM Products;
            
            -- Create POs
			SELECT
				p.Id AS ProductId,
				NEWID() AS PurchaseOrderId,
				CONCAT('PO-', FORMAT(ROW_NUMBER() OVER (ORDER BY p.Id, n.n), 'D7')) AS OrderNumber,
				DATEADD(DAY, -n.n % 60, SYSUTCDATETIME()) AS OrderDate,
				DATEADD(DAY, (n.n % 30) + 3, SYSUTCDATETIME()) AS ExpectedDeliveryDate--,
				--NULL AS ReceivedDate
			INTO #PO_Source
			FROM Products p
			JOIN #Numbers n ON n.n < @NumberOfPOs;
			
			INSERT INTO PurchaseOrder (
				Id,
				OrderNumber,
				[Status],
				OrderDate,
				ExpectedDeliveryDate,
				--ReceivedDate,
				CompanyId
			)
			SELECT
				PurchaseOrderId,
				OrderNumber,
				0,
				OrderDate,
				ExpectedDeliveryDate,
				--ReceivedDate,
				@CompanyId
			FROM #PO_Source
			
			INSERT INTO PurchaseOrderLineItem (
				PurchaseOrderId,
				ProductId,
				Quantity,
				ReceivedQuantity,
				UnitCost
			)
			SELECT
				PurchaseOrderId,
				ProductId,
				q.qty,
				0,
				p.Price
			FROM #PO_Source s
			JOIN Products p ON p.Id = s.ProductId
			CROSS APPLY (
				SELECT ABS(CHECKSUM(NEWID())) % 46 + 5 AS qty
			) q;

			DROP TABLE #PO_Source;
            
            -- Create COs
			SELECT
				p.Id AS ProductId,
				NEWID() AS ConfirmedOrderId,
				CONCAT('CO-', FORMAT(ROW_NUMBER() OVER (ORDER BY p.Id, n.n), 'D7')) AS OrderNumber,
				DATEADD(DAY, n.n % 60, SYSUTCDATETIME()) AS OrderDate,
				DATEADD(DAY, (n.n % 30) + 3, SYSUTCDATETIME()) AS ExpectedDeliveryDate--,
				--NULL AS ReceivedDate
			INTO #CO_Source
			FROM Products p
			JOIN #Numbers n ON n.n < @NumberOfCOs;

			INSERT INTO ConfirmedOrder (
				Id,
				OrderNumber,
				Status,
				OrderDate,
				ExpectedDeliveryDate,
				--ReceivedDate,
				CompanyId
			)
			SELECT
				ConfirmedOrderId,
				OrderNumber,
				0,
				OrderDate,
				ExpectedDeliveryDate,
				--ReceivedDate,
				@CompanyId
			FROM #CO_Source;
            
			INSERT INTO ConfirmedOrderLineItem (
				ConfirmedOrderId,
				ProductId,
				Quantity,
				SoldQuantity,
				UnitCost
			)
			SELECT
				ConfirmedOrderId,
				ProductId,
				q.qty,
				0,
				p.Price
			FROM #CO_Source s
			JOIN Products p ON p.Id = s.ProductId
			CROSS APPLY (
				SELECT ABS(CHECKSUM(NEWID())) % 46 + 5 AS qty
			) q;

			DROP TABLE #CO_Source;
            
            -- Create transactions
			SELECT
				p.Id AS ProductId,
				NEWID() AS TransactionGuidId,
				CONCAT('T-', FORMAT(ROW_NUMBER() OVER (ORDER BY p.Id, ABS(CHECKSUM(NEWID()))), 'D8')) AS TransactionId,
				DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 1095, SYSUTCDATETIME()) AS TransactionDate,
				CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN 'Card' ELSE 'Cash' END AS PaymentMethod,
				NULL AS Notes,
				0 AS TotalAmount
			INTO #Tran_Source
			FROM Products p
			JOIN #Numbers n ON n.n < 50;

			INSERT INTO Transactions (
				Id,
				TransactionId,
				TransactionDate,
				PaymentMethod,
				Notes,
				TotalAmount,
				CompanyId
			)
			SELECT
				TransactionGuidId,
				TransactionId,
				TransactionDate,
				PaymentMethod,
				Notes,
				TotalAmount,
				@CompanyId
			FROM #Tran_Source;
            
			INSERT INTO TransactionLineItems (
				TransactionId,
				ProductId,
				Quantity,
				UnitPrice
			)
			SELECT
				s.TransactionGuidId,
				s.ProductId,
				ABS(CHECKSUM(NEWID())) % 46 + 5,
				p.Price
			FROM #Tran_Source s
			JOIN Products p ON p.Id = s.ProductId

			DROP TABLE #Tran_Source;
            
            -- Drop Numbers table
            DROP TABLE #Numbers

			IF @EnsurePositiveStock = 1
			BEGIN
				EXEC dbo.CreateFulfilledPOs 
					@CompanyId = @CompanyId,
					@SafetyBuffer = 50,
					@NumberOfProducts = @NumberOfProducts
			END

        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
