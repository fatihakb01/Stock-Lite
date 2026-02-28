# Database Documentation

This document describes the database layer of Stock-Lite, with a focus on:
- User-defined types
- Stored procedures
- Calculation jobs
- Import jobs
- How derived entities are generated and updated

The database plays a critical role in the system. While the application layer orchestrates jobs, heavy data calculations are intentionally pushed to SQL Server for:
- Performance
- Atomic operations
- Reduced data transfer
- Set-based computation

# Calculation Jobs

## 1. User Defined Types

### dbo.GuidList

#### Purpose

A table-valued parameter (TVP) used to pass multiple ProductIds into stored procedures.

#### Definition
```bash
CREATE TYPE dbo.GuidList AS TABLE (Id UNIQUEIDENTIFIER);
```

#### Why it exists

Many calculation procedures optionally operate on:
- All products of a company
- A subset of products

Instead of looping in application code, a TVP allows efficient filtering directly in SQL.

## 2. Sales Aggregates

Sales aggregates summarize transactional sales data into:
- Daily
- Weekly
- Monthly

These aggregates are used as the foundation for:
- Seasonality
- Forecasts

### 2.1 InsertSalesAggregates

#### Procedures
- `dbo.InsertDailySalesAggregates`
- `dbo.InsertWeeklySalesAggregates`
- `dbo.InsertMonthlySalesAggregates`
- `dbo.InsertSalesAggregates` (wrapper)

#### Purpose

Creates missing SalesAggregate records for:
- Last 30 days (Daily)
- Last 12 weeks (Weekly)
- Last 36 months (Monthly)

If a SalesAggregate record does not exist for a product + period, it is inserted with:
```bash
TotalQuantity = 0
TotalRevenue = 0
```

This guarantees that all periods exist before update calculations run.

Parameters
- `@CompanyId` = Company scope
- `@ProductIds` = Optional list of ProductIds

### 2.2 UpdateSalesAggregates

#### Procedures
- `dbo.UpdateDailySalesAggregates`
- `dbo.UpdateWeeklySalesAggregates`
- `dbo.UpdateMonthlySalesAggregates`
- `dbo.UpdateSalesAggregates` (wrapper)

#### Purpose

Updates aggregate values based on `TransactionLineItems` and `Transactions`.

#### Daily Logic

- Groups sales per: `ProductId` + `TransactionDate`
- Updates: `TotalQuantity` and `TotalRevenue`

Weekly Logic

- Groups sales between: `WeekStartDate` → `WeekStartDate + 6` days

Monthly Logic
- Groups sales between: First day of month → End of month

#### Design Decision

Insert and Update are separated intentionally:
- Insert ensures period completeness
- Update ensures data accuracy
- Wrapper procedures guarantee execution order

## 3. Seasonality

Seasonality adjusts forecasts based on historical monthly behavior.

### 3.1 DeletePastSeasonality

Removes seasonality records for past months.

Purpose:
- Keep only relevant periods
- Avoid stale seasonality data

### 3.2 InsertSeasonality

Creates seasonality entries for:
- Previous month
- Current month
- Next 11 months

Default:
```bash
Factor = 1
```

This ensures seasonality always exists before update logic runs.

### 3.3 UpdateSeasonality

#### Logic

1. Collect monthly sales aggregates
2. Calculate:
    - Overall product average
    - Average per month (Jan, Feb, etc.)
3. Compute factor:
```bash
Factor = MonthAverage / OverallAverage
```

#### Safeguards

If:
- Less than 12 months of data
- Overall average = 0

Then:
```bash
Factor = 1
```
This prevents unstable seasonality values.

## 4. Forecasts

Forecasts are calculated in three levels:
- Monthly
- Weekly
- Daily

They are hierarchical:
```bash
Monthly → Weekly → Daily
```

### 4.1 DeletePastForecasts

Deletes forecast entries where:
```bash
PeriodEndDate < Today
```
Executed per period type before new forecasts are inserted.

### 4.2 InsertForecasts

#### Procedures
- `InsertMonthlyForecasts`
- `InsertWeeklyForecasts`
- `InsertDailyForecasts`
- `InsertForecasts` (wrapper)

#### Purpose

Ensures forecast records exist for future periods:
- Monthly: -1 to +11 months
- Weekly: -1 to +51 weeks
- Daily: 0 to +29 days

Initial values:
```bash
PredictedQuantity = 0
PredictedRevenue = 0
Confidence = 0.50
```

### 4.3 UpdateMonthlyForecasts

#### Logic
1. Take last 12 months of SalesAggregates
2. Calculate:
    - Average Quantity
    - Average Revenue
3. Multiply by Seasonality factor

```bash
PredictedQuantity = AvgQty × SeasonalityFactor
PredictedRevenue = AvgRevenue × SeasonalityFactor
```

Confidence is currently static:
```bash
0.50
```

### 4.4 UpdateWeeklyForecasts

#### Logic
1. Start from Monthly forecasts
2. Determine overlap between:
    - Weeks
    - Months
3. Weight monthly prediction into weekly buckets

Key principle:

```bash
Weekly = Weighted portion of Monthly
```

### 4.5 UpdateDailyForecasts

#### Logic
1. Start from Weekly forecasts
2. Divide evenly across 7 days

```bash
Daily = Weekly / 7
```

### 4.6 UpdateForecasts (Wrapper)

Ensures order:
```bash
Monthly → Weekly → Daily
```

This preserves hierarchical forecast consistency.

## 5. Confirmed Order Aggregates

`ConfirmedOrderAggregates` summarize committed customer demand into time buckets.

While `Forecast` reflect expected future sales, `ConfirmedOrderAggregates` represent future committed demand.

This separation ensures:
- Using historical trend analysis to predict the future (`Forecast`)
- Forward-looking confirmed demand (`ConfirmedOrderAggregates`)
- Accurate planning calculations

### 5.1 InsertConfirmedOrderAggregates

#### Purpose

Ensures aggregate records exist for future periods before updates run.

Periods supported:
- Daily
- Weekly
- Monthly

If no aggregate exists for: `ProductId` + `PeriodStartDate` + `PeriodType`

Then a new record is inserted with:

```bash
TotalConfirmedQuantity = 0
```

This guarantees period completeness.

### 5.2 UpdateConfirmedOrderAggregates

#### Logic
1. Collect `ConfirmedOrderLineItems`
2. Filter by:
    - `CompanyId`
    - Optional `@ProductIds (dbo.GuidList)`
3. Group by:
    - `ProductId`
    - `Period` (Daily / Weekly / Monthly)
4. Update:
    - `TotalConfirmedQuantity`

#### Design Purpose

Confirmed demand is separated from forecast demand to allow:
- Forecast vs committed demand comparison
- Better purchase planning
- More accurate stock projection

## 6. Purchase Forecast

PurchaseForecast is a planning entity that determines:
- When to reorder
- How much to reorder
- Based on forecast + confirmed demand + stock policies

This is one of the most complex calculation procedures in the system.

### 6.1 UpdatePurchaseForecast

#### Input Data

The procedure combines:
- `Forecasts`
- `ConfirmedOrderAggregates`
- Current Stock Levels
- Open Purchase Orders
- `ProductInventoryPolicies`:
    - `LeadTimeDays`
    - `SafetyStock`
    - `MinimumOrderQuantity`
    - `OrderIncrementQuantity`

### 6.2 Calculation Logic

The procedure performs the following steps:

**1 Define Forecast Window**

```bash
Today → Today + LeadTimeDays
```

**2 Calculate Total Demand Inside Window**

Demand consists of:

```bash
Forecasted Demand + Confirmed Demand
```

**3 Project Stock Level**

```bash
ProjectedStock = CurrentStock
+ IncomingPurchaseOrders
- DemandInWindow
```

**4 Determine Reorder Need**

If:

```bash
ProjectedStock < SafetyStock
```

Then reorder is required.

**5 Calculate Recommended Quantity**

Steps:
- Determine shortage relative to safety stock
- Respect MOQ
- Round up to nearest OrderIncrementQuantity

**6 Determine Recommended Order Date**

```bash
Today - LeadTimeDays
```

This ensures stock arrives before projected shortage.

### 6.3 Design Rationale

This procedure is intentionally implemented in SQL because:
- It is set-based
- It operates on many products simultaneously
- It avoids excessive memory transfer to the API
- It guarantees atomic calculation

The result populates the PurchaseForecast table.

## 7. Stock Levels

StockLevel represents the current and projected stock situation per product.

It is recalculated through a dedicated procedure.

### 7.1 UpdateStockLevels

#### Logic

For each Product:

**1 Calculate Current Stock**

```bash
SUM(InboundTransactions) - SUM(OutboundTransactions)
```

**2 Add:**

```bash
Open Purchase Orders
```

**3 Subtract:**

```bash
Confirmed Orders
```

**4 Store:**
- CurrentStock
- IncomingStock
- ReservedStock
- AvailableStock

#### 7.2 Purpose

`StockLevel` acts as:
- A fast lookup table
- A planning baseline

By persisting stock levels instead of recalculating on every query:
- Performance improves
- Queries remain lightweight

## 8. Data Seeder

The `DataSeederInitializer` is responsible for installing and maintaining database-side initialization and demo-data procedures.

Unlike the calculation procedures (which compute derived data), the seeder focuses on:
- Bootstrapping master data
- Generating realistic demo datasets
- Resetting environments safely
- Supporting performance and stress testing

### 8.1 Daily Records Procedures

The following procedures generate randomized daily datasets for dashboard previews, simulations, or testing:
- `dbo.DailyTransactionRecords`
- `dbo.DailyPoRecords`
- `dbo.DailyCoRecords`
- `dbo.DailyProductRecords`

#### Purpose

They dynamically return mixed datasets consisting of:
- 90% newly generated records
- 10% sampled existing records

This creates:
- Realistic variance
- Fresh-looking data
- Stable references to existing data

#### Design Characteristics
- Randomized quantities using `CHECKSUM(NEWID())`
- Randomized dates within controlled ranges
- Mixed historical and new data
- Ordered randomly using `ORDER BY NEWID()`

These procedures do not persist data — they generate dynamic result sets for simulation and preview purposes.

### 8.2 CreateCompany

#### Procedure

`dbo.CreateCompany`

#### Purpose

Bootstraps a fully functional company environment in a single atomic operation.

#### What It Creates
- Company record
- SuperAdmin user
- Default JobTasks
- Pre-configured ScheduledJobs

#### Scheduled Jobs Created

The procedure inserts scheduled jobs for:
- Product import
- Transaction import
- Confirmed order import
- Purchase order import
- Sales calculation
- Stock level calculation
- Forecast calculation
- Confirmed demand calculation
- Purchase forecast calculation
- Product KPI calculation

#### Design Rationale

By creating jobs directly in SQL:
- The system becomes self-initializing
- No manual configuration is required
- New tenants can be provisioned instantly
- All required job dependencies are guaranteed

This procedure acts as a tenant bootstrap mechanism.

### 8.3 CleanMasterData

#### Procedure

`dbo.CleanMasterData`

#### Purpose

Resets transactional and master product data while preserving structural and configuration data.

#### What It Deletes
- `TransactionLineItems`
- `Transactions`
- `ConfirmedOrderLineItem`
- `ConfirmedOrder`
- `PurchaseOrderLineItem`
- `PurchaseOrder`
- `Products`

#### Design Intent
- Enables full reseeding
- Allows performance testing with different dataset sizes
- Keeps company/user/job configuration intact

This procedure is used internally by `SeedData`.

### 8.4 CreateFulfilledPOs

#### Procedure

`dbo.CreateFulfilledPOs`

#### Purpose

Automatically generates fulfilled purchase orders for products with zero or negative stock.

#### Logic Overview

Identify products where:

```bash
QuantityOnHand <= 0
```

Create backdated PurchaseOrders with:
- Status = Fulfilled
- ReceivedDate = Today

Insert `PurchaseOrderLineItems` where:

```bash
Quantity = ABS(Deficit) + SafetyBuffer
```

#### Why It Exists

After large data seeding operations, some products may end up with negative stock due to:
- Random transaction generation
- Confirmed orders exceeding inventory

This procedure:
- Stabilizes inventory
- Prevents unrealistic negative stock scenarios
- Ensures planning calculations start from valid conditions

### 8.5 SeedData

#### Procedure

`dbo.SeedData`

This is the primary demo and performance dataset generator.

#### Parameters
- `@CompanyId`
- `@EnsurePositiveStock`
- `@NumberOfProducts`
- `@NumberOfPOs`
- `@NumberOfCOs`
- `@NumberOfTransactions`

#### Execution Flow

**1 Clean existing master data**

```bash
EXEC dbo.CleanMasterData
```

**2 Generate Products**
- Random price
- Random active state
- Random creation date


**3 Generate ProductInventoryPolicies**
- Random LeadTimeDays
- Random MOQ
- Random SafetyStock
- `OrderIncrementQuantity = 1`

**4 Generate Purchase Orders**
- Open POs
- Random quantities
- Future expected delivery dates

**5 Generate Confirmed Orders**
- Open customer demand
- Random quantities
- Future expected delivery dates

**6 Generate Transactions**
- Random historical sales
- Random quantities
- Random payment methods
- Spread over ~3 years

**7 Optional Stock Stabilization**

If:

```bash
@EnsurePositiveStock = 1
```

Then:

```bash
EXEC dbo.CreateFulfilledPOs
```

#### Design Characteristics
- Uses temporary number tables (#Numbers)
- Uses set-based inserts for scalability
- Supports large datasets (100k+ products)
- Avoids row-by-row logic
- Designed for performance benchmarking

### 8.6 Architectural Rationale

The `DataSeederInitializer` exists for three main reasons:

#### 1. Reproducible Environments

Every environment (local, staging, demo) can be recreated deterministically.

#### 2. Performance Testing

Large datasets allow:
- Execution plan validation
- Index tuning
- Calculation stress testing
- Forecast scalability validation

#### 3. Development Velocity

Developers can:
- Reset data instantly
- Generate realistic planning scenarios
- Test purchase forecast behavior
- Validate edge cases (negative stock, high demand spikes, etc.)

### 8.7 Relationship to Calculation Procedures

The seeder does not perform business calculations itself.

Instead, it prepares datasets that are later consumed by:
- Sales aggregate calculations
- Confirmed order aggregation
- Forecast calculations
- Stock level calculations
- Purchase forecast calculations

This separation ensures:
- Clean responsibility boundaries
- Deterministic data generation
- Independent evolution of seeding vs planning logic

# Import Jobs

Import jobs are responsible for loading external data (CSV files) into the system in a controlled, consistent, and idempotent way.

Each import follows the same architectural pattern:

**1 Staging table**
- Raw data is first inserted into a dedicated `dbo.Staging` table.
- No business logic is executed at this stage.
- The staging table mirrors the structure of the external file.

**2 Process stored procedure**
- A `dbo.Process` procedure transforms staging data into domain tables.
- Handles:
    - Insert of new records
    - Update of existing records
    - Insert/update of related line items
    - Status calculation (when applicable)
    - Cleans up staging data after processing

This design ensures:
- Idempotent imports (safe re-processing)
- Clear separation between raw input and domain data
- Atomic, set-based processing
- Minimal API memory footprint
- Multi-tenant isolation via `@CompanyId`

## 1. Confirmed Order Import

### Staging Table

`dbo.StagingConfirmedOrders`

Stores raw confirmed order data:
- `CompanyId`
- `OrderNumber`
- `SKU`
- `OrderDate`
- `ExpectedDeliveryDate`
- `ReceivedDate`
- `Quantity`
- `SoldQuantity`

### Processing Procedure

`dbo.ProcessConfirmedOrders(@CompanyId)`

#### Responsibilities

1. Insert missing ConfirmedOrder records
    - Groups staging rows by `OrderNumber`
    - Derives status based on:

        ```bash
        Quantity - SoldQuantity
        ```

    - Inserts only when order does not exist

2. Update existing orders
    - Updates dates and recalculates status

3. Insert missing `ConfirmedOrderLineItem` records
    - Resolves `ProductId` via `SKU`
    - Inserts only if line item does not exist
4. Update existing line items
    - Updates `Quantity`, `SoldQuantity`, and `UnitCost`
5. Cleanup
    - Deletes processed staging rows for the company

### Design Purpose
- Supports partial deliveries
- Maintains correct order status
- Prevents duplicate orders or line items

## 2. Product Import

### Staging Table

`dbo.StagingProducts`

Stores:
- `CompanyId`
- `SKU`
- `Name`
- `Price`
- `Description`
- `IsActive`
- `CreatedAt`
- `LeadTimeDays`
- `MinimumOrderQuantity`
- `OrderIncrementQuantity`
- `SafetyStock`
- `StockAdjustment`

### Processing Procedure

`dbo.ProcessProducts(@CompanyId)`

#### Responsibilities

**1 Insert missing Products**
- Based on `CompanyId` + `SKU` uniqueness

**2 Update existing Products**
- Updates `Name`, `Price`, `Description`, `IsActive`, `CreatedAt`

**3. Insert missing ProductInventoryPolicies**
- Creates policy record if none exists

**4. Update existing ProductInventoryPolicies**
- Updates lead time, MOQ, safety stock, IOQ

**5. Cleanup staging**

#### Design Purpose
- Keeps master data consistent
- Ensures every product has inventory policies
- Allows policy updates through import files

## 3. Purchase Order Import

### Staging Table

`dbo.StagingPurchaseOrders`

Stores:
- `CompanyId`
- `OrderNumber`
- `SKU`
- `OrderDate`
- `ExpectedDeliveryDate`
- `ReceivedDate`
- `Quantity`
- `ReceivedQuantity`

### Processing Procedure

`dbo.ProcessPurchaseOrders(@CompanyId)`

#### Responsibilities
**1 Insert missing PurchaseOrder records**
- Groups by `OrderNumber`
- Calculates status based on:

```bash
Quantity - ReceivedQuantity
```

**2 Update existing orders**
- Updates dates and recalculates status

**3 Insert missing PurchaseOrderLineItem records**
- Resolves `ProductId` by `SKU`

**4 Update existing line items**
- Updates `Quantity`, `ReceivedQuantity`, `UnitCost`

**5 Cleanup staging**

#### Design Purpose
- Supports partial receipts
- Maintains accurate PO status
- Keeps receiving logic centralized in SQL

## 4. Transaction Import

### Staging Table

`dbo.StagingTransactions`

Stores:
- `CompanyId`
- `TransactionId`
- `SKU`
- `TransactionDate`
- `PaymentMethod`
- `Notes`
- `Quantity`

### Processing Procedure

`dbo.ProcessTransactions(@CompanyId)`

#### Responsibilities
**1 Insert missing Transactions**
- Groups by TransactionId
- Calculates:

```bash
TotalAmount = SUM(Quantity × Product.Price)
```

**2 Update existing Transactions**
- Recalculates total amount
- Updates date, payment method, notes

**3 Insert missing TransactionLineItems**
- Resolves ProductId by SKU

**4 Update existing line items**
- Updates Quantity and UnitPrice

**5 Cleanup staging**

### Design Purpose
- Guarantees transaction totals remain consistent with line items
- Supports re-import without duplication
- Enables accurate sales aggregates downstream

## 5. User Defined Field Import

### Staging Table

`dbo.StagingUserDefinedFields`

Stores:
- `CompanyId`
- `SKU`
- `Key`
- `Value`

### Processing Procedure

`dbo.ProcessUserDefinedFields(@CompanyId)`

### Responsibilities
**1 Insert new UserDefinedFields**
- Based on ProductId + Key uniqueness

**2 Update existing keys**
- Updates Value when key already exists

**3. Cleanup staging**

### Design Purpose
- Allows flexible, extensible product attributes
- Avoids schema changes for additional metadata
- Maintains product-level custom fields per company

## Import Architecture Summary

All import jobs follow a consistent pattern:

```bash
External File
     ↓
Staging Table
     ↓
Process Stored Procedure
     ↓
Domain Tables
     ↓
Staging Cleanup
```

### Key Architectural Decisions
- All imports are multi-tenant (`@CompanyId`)
- Inserts and updates are handled separately
- Business rules (status calculation, totals, relationships) are executed in SQL
- Processing is fully set-based
- Staging tables act as isolation buffers

This approach ensures:
- Performance
- Data integrity
- Repeatable imports
- Clear separation between raw data and domain logic
- Stability for downstream calculation jobs (forecast, seasonality, stock levels, purchase planning)

The import layer acts as the foundation for all calculation jobs described earlier in this document.
