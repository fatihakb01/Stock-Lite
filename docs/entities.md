# Entities Overview

This document describes the domain entities of Stock-Lite, their responsibilities, and the relationships between them.

Entities are divided into two main categories:
- Core Entities – Fundamental building blocks of the system
- Derived Entities – Calculated or aggregated data derived from core entities

## 🏢 Core Entities

Core entities represent the operational data required for the system to function.

### 🏬 Company (Aggregate Root)

The Company entity is the top-level aggregate root of the system.

A company owns:
- Users
- Products
- Transactions
- Purchase Orders
- Confirmed Orders
- Job Tasks
- Product KPIs

Relationships
- 1 Company → Many Users
- 1 Company → Many Products
- 1 Company → Many Transactions
- 1 Company → Many PurchaseOrders
- 1 Company → Many ConfirmedOrders
- 1 Company → Many JobTasks
- 1 Company → 1 ProductKpi

All operational data is scoped per company.

### 👤 User

User represents an authenticated system user.
- Extends `IdentityUser`
- Belongs to exactly one Company
- Has a Role (enum)

Relationship
- Many Users → 1 Company

### 📦 Product Domain

Product represents a sellable item. A product belongs to exactly one company and is central to forecasting and stock calculations.

Relationships
- Many Products → 1 Company
- 1 Product → 1 ProductInventoryPolicy
- 1 Product → 1 StockLevel
- 1 Product → Many PurchaseForecasts
- 1 Product → Many Forecasts
- 1 Product → Many SalesAggregates
- 1 Product → Many Seasonality records
- 1 Product → Many TransactionLineItems
- 1 Product → Many PurchaseOrderLineItems
- 1 Product → Many ConfirmedOrderLineItems
- 1 Product → Many ConfirmedOrderAggregates
- 1 Product → Many UserDefinedFields

The Product entity is a central aggregation point for operational and analytical data.

### 📑 ProductInventoryPolicy

Product inventory policy defines replenishment configuration for a product.

Contains:
- Lead time
- Minimum order quantity
- Order increments
- Safety stock
- Stock adjustment

Relationship
- 1 ProductInventoryPolicy → 1 Product

### 🏷 UserDefinedField

Allows dynamic/custom fields per product.

Relationship
- Many UserDefinedFields → 1 Product

### 🧾 Transactions (Sales)

Transaction represents a sales transaction. Belongs to a company and contains line items.

Relationship
- Many Transactions → 1 Company
- 1 Transaction → Many TransactionLineItems

### 🧾 TransactionLineItem

Transaction Line Item represents a product sold in a transaction.

Relationship
- Many TransactionLineItems → 1 Transaction
- Many TransactionLineItems → 1 Product

This creates a many-to-many relationship between Transactions and Products via line items.

### 🏗 Purchase Orders

Purchase Order represents an internal purchase order (planned procurement).

Relationship
- Many PurchaseOrders → 1 Company
- 1 PurchaseOrder → Many PurchaseOrderLineItems

### 📦 PurchaseOrderLineItem

Purchase Order Line Item represents a product within a purchase order.

Relationship
- Many PurchaseOrderLineItems → 1 PurchaseOrder
- Many PurchaseOrderLineItems → 1 Product

### 🚚 Confirmed Orders

Confirmed orders represent externally confirmed supply orders.

Relationship
- Many ConfirmedOrders → 1 Company
- 1 ConfirmedOrder → Many ConfirmedOrderLineItems

### 📦 ConfirmedOrderLineItem

Confirmed Order Line Item represents a product within a confirmed order.

Relationship
- Many ConfirmedOrderLineItems → 1 ConfirmedOrder
- Many ConfirmedOrderLineItems → 1 Product

### ⏱ Scheduler Domain

The scheduler is responsible for automated jobs such as forecasting and imports.

#### 🗂 JobTask

Logical grouping of scheduled jobs.

Relationship
- Many JobTasks → 1 Company
- 1 JobTask → Many ScheduledJobs

#### 🕒 ScheduledJob

Represents a specific scheduled job configuration and contains:
- Job type
- Calculation type
- Import type
- Period type
- Execution order

Relationship
- Many ScheduledJobs → 1 JobTask
- 1 ScheduledJob → Many JobExecutions

#### ▶ JobExecution

Represents a single execution instance of a scheduled job.

Relationship
- Many JobExecutions → 1 ScheduledJob

## 📊 Derived Entities

Derived entities represent calculated, aggregated, or analytical data. They are typically produced by stored procedures or scheduled jobs.

### 📈 SalesAggregate

Aggregated sales per product and period.

Relationship
- Many SalesAggregates → 1 Product

### 📦 ConfirmedOrderAggregate

Aggregated confirmed supply quantities per product and period.

Relationship
- Many ConfirmedOrderAggregates → 1 Product

### 🔮 Forecast

Predicted sales quantities per product and period.

Contains:
- Predicted quantity
- Predicted revenue
- Confidence level

Relationship
- Many Forecasts → 1 Product

### 📊 Seasonality

Represents seasonal adjustment factors per product.

Relationship
- Many Seasonality records → 1 Product

### 📅 PurchaseForecast

Weekly purchasing projection per product.

Contains:
- Forecast quantity
- Committed quantity
- On-order quantity
- Projected stock
- Reorder recommendation

Relationship
- Many PurchaseForecasts → 1 Product

### 📦 StockLevel

Represents calculated stock state of a product.

Contains:
- Quantity on hand
- Quantity on order
- Quantity committed
- Reorder point
- Recommended order quantity

Relationship
- 1 StockLevel → 1 Product

### 📊 ProductKpi

Company-wide product metrics.

Contains:
- Total products
- Products below safety stock
- Recommended orders
- Missing forecast count
- Scheduler status

Relationship
- 1 ProductKpi → 1 Company

## 🔗 High-Level Relationship Summary

At a high level:
- Company is the top-level owner of operational data.
- Product is the central entity for forecasting and stock optimization.
- Transactions and Orders generate operational data.
- Derived entities transform operational data into analytical insights.
- Scheduler entities automate calculations and data imports.
