# API Layer

## Overview

The API layer is responsible for exposing the system’s functionality over HTTP.

It acts as a thin orchestration layer that:
- Receives HTTP requests
- Validates authentication and authorization
- Maps requests to Application layer commands and queries
- Returns standardized HTTP responses

The API layer does not contain business logic. All domain logic resides in the Application and Domain layers.

## Architectural Principles

The API layer follows these principles:
- Thin controllers (no business logic)
- CQRS via MediatR
- DTO-based input/output models
- Policy-based authorization
- Centralized result handling
- Clean separation from Domain and Infrastructure logic

Controllers only depend on:
- MediatR
- Application DTOs
- ASP.NET Core framework components

## BaseApiController

All controllers inherit from BaseApiController.

Responsibilities

**1. Centralized Mediator Access**

Each controller receives an injected IMediator instance and uses it to dispatch commands and queries to the Application layer.

```bash
await Mediator.Send(new SomeCommand());
```

This ensures:
- Controllers remain thin
- No direct dependency on services or repositories
- All business logic is executed via Application handlers

**2. Standardized Result Handling**

All responses are passed through:

```bash
HandleResult<T>(Result<T> result)
```

This method:
- Returns 200 OK when successful with value
- Returns 400 BadRequest when validation fails
- Returns ProblemDetails with appropriate status code for failures

This enforces consistent HTTP behavior across the entire API.

## Authentication & Authorization

The API uses policy-based authorization.

### Global Rule

Most controllers require authentication:

```bash
[Authorize]
```

### Policies

Two main policies are used:
- `AdminOrSuperAdmin`
- `SuperAdminOnly`
- `KeyUserOrAdminOrSuperAdmin`

This ensures:
- Role-based access control
- Administrative endpoints are restricted
- Sensitive operations are protected

Example:
- Creating or editing entities → Admin or SuperAdmin
- Running jobs → SuperAdmin only
- Company creation → SuperAdmin only
- Running exports → KeyUser or Admin or SuperAdmin

User authentication endpoints (login, confirm email, reset password) allow anonymous access where required.

## Controller Structure

Each entity has its own controller.

Controllers follow REST-like conventions.

Each controller:
- Uses DTOs for input/output
- Dispatches Commands for mutations
- Dispatches Queries for reads
- Returns strongly typed `ActionResult<T>`

## Core Entity Controllers

### CompanyController

Manages company-level configuration.

Endpoints:
- GET → Retrieve company
- POST → Create company (SuperAdmin)
- PUT → Edit company (SuperAdmin)

### ProductController

Manages products and inventory policies.

Endpoints:
- GET (list with optional filter)
- GET by Id
- POST → Create product (Admin+)
- PUT → Edit product (Admin+)
- DELETE → Remove product (SuperAdmin)

### ConfirmedOrderController

Handles customer confirmed orders.

Endpoints:
- GET all
- GET by product
- POST create (Admin+)
- POST receive
- POST cancel
- PUT update

Implements explicit domain actions via route-based commands:
- `/receive`
- `/cancel`

This avoids overloading PUT for state transitions.

### PurchaseOrderController

Handles supplier purchase orders.

Endpoints:
- GET all
- GET by product
- POST create
- POST receive
- POST submit
- POST cancel
- PUT update

Explicit command routes reflect domain behavior.

### TransactionController

Handles sales transactions.

Endpoints:
- GET all
- GET by product
- POST create
- PUT edit
- DELETE remove

### UserController

Handles authentication and user management.

Endpoints include:
- POST login
- POST register (SuperAdmin)
- POST confirm-email
- POST reset-password
- POST change-password
- GET current user
- GET all users
- GET user by id
- PUT edit
- DELETE remove

Authentication-related endpoints allow anonymous access where appropriate.

### JobTaskController

Manages executable background tasks.

Endpoints:
- GET list
- GET by id
- GET history
- POST run
- POST create
- PUT edit
- DELETE remove

SuperAdmin access required for execution and modification.

### ScheduledJobController

Manages job scheduling and ordering.

Endpoints:
- GET by id
- GET history
- POST run
- POST reorder
- POST create
- PUT edit
- DELETE remove

Allows dynamic reordering of scheduled jobs.

### UserDefinedFieldController

Retrieves dynamic product-level metadata.

Endpoint:
- GET by product id

Supports flexible extensibility without schema changes.

### ExportProfileController

Manages running exports

Endpoints:
- GET list
- GET by id
- POST run
- POST stream
- POST create
- PUT edit
- DELETE remove

Distinction between saving at pre-defined target path (run) and downloading from browser (stream).

## Derived Entity Controllers

In addition to core entity controllers, the API layer exposes a set of derived entity controllers.

Derived entities are not directly created or modified by users.
They are calculated, aggregated, or projected from core domain data such as:
- Transactions
- Confirmed orders
- Purchase orders
- Inventory policies

These controllers expose:
- Analytical data (aggregates, trends, KPIs)
- Forecasting results
- Planning projections
- Calculated stock metrics

All derived entities follow the same pattern:
- GET endpoints → Retrieve calculated results
- POST endpoints → Trigger recalculation/update jobs (Admin or SuperAdmin only)

This enforces a clear separation between:
- Core transactional data
- Analytical / computed projections

### Architectural Characteristics

Derived controllers:
- Follow CQRS via MediatR
- Never contain calculation logic
- Delegate heavy computation to the Application layer
- Often trigger set-based database calculations (stored procedures)

Support optional filtering by:
- productId
- periodType
- A list of product IDs

These controllers represent the analytics and planning surface of the system.

### ConfirmedOrderAggregateController

Provides aggregated views of confirmed orders over time.

**GET**

```bash
GET /{periodType}/{productId}
```

Returns confirmed order outlook grouped by:
- Day
- Week
- Month

**POST (Admin+)**

```bash
POST /{periodType?}
```

Triggers recalculation of confirmed order aggregates:
- For specific products
- Or for all products (when no IDs provided)

Used to refresh analytical projections after data imports or transactional updates.

### SalesAggregateController

Provides sales trend analytics derived from transactions.

**GET**

```bash
GET /{periodType}/{productId}
```

Returns aggregated sales trends by period type.

**POST (Admin+)**

```bash
POST /{periodType?}
```

Triggers recalculation of sales aggregates.

Used for:
- Trend/historical analysis
- Feeding forecast models

### ForecastController

Generates and retrieves sales forecasts.

**GET**

```bash
GET /{periodType}/{productId}
```

Returns forecasted demand values for a product.

**POST (Admin+)**

```bash
POST /{periodType?}
```

Triggers forecast generation:
- For specific products
- Or globally

Forecast generation depends on historical aggregates and seasonality calculations.

### SeasonalityController

Calculates seasonality factors per product.

**GET**

```bash
GET /{productId}
```

Returns seasonality coefficients used in forecasting models.

**POST (Admin+)**
- Triggers recalculation of seasonality values
- Optional filter for ProductIds

Seasonality is typically derived from long-term sales patterns.

### ProductKpiController

Provides products adhering to a specific condition.

**GET**

Returns number of products adhering to a specific condition:
- Total products
- Number of products below safety stock
- Number of products with recommended orders
- Number of products without stock at lead time
- Number of products without forecast

**POST**
- Triggers recalculation of product KPIs.
- Unlike other derived controllers, this one operates at a broader summary level rather than per product ID.

### StockLevelController

Calculates real-time stock positions.

**GET**

```bash
GET /{productId}
```

Returns calculated stock metrics, such as:
- Quantity on Hand
- Quantity on Order
- Quantity Committed
- Net Stock Position
- Recommended Order Quantity

**POST (Admin+)**

Triggers recalculation of stock levels for:
- Selected products
- Or all products

This ensures stock levels remain consistent after imports or order changes.

### PurchaseForecastController

Provides procurement planning projections.

**GET**

```bash
GET /{productId}
```

Returns calculated purchase forecasts, typically based on:
- Effective demand
- Lead times
- Safety stock
- Current stock levels

**POST (Admin+)**
- Triggers recalculation of purchase forecasts.
- This controller bridges forecasting and purchasing decisions.

## CQRS Pattern

The API strictly follows CQRS:
- Queries → Return data
- Commands → Modify state

Controllers never access repositories or DbContext directly.

All requests flow:

```bash
HTTP Request
    ↓
Controller
    ↓
MediatR
    ↓
Application Handler
    ↓
Domain / Infrastructure
```

## DTO Usage

The API never exposes Domain entities directly.

Instead:
- Input DTOs validate incoming data
- Output DTOs shape responses
- Prevents domain leakage
- Enables versioning flexibility

## Error Handling Strategy

Error handling is centralized through the `Result<T>` pattern.

Application layer returns:

```bash
Result.Success(value)
Result.Failure(error, statusCode)
```

The API converts this into:
- 200 OK
- 400 Bad Request
- Proper HTTP status codes
- ProblemDetails when needed

This ensures consistent API behavior.

## Design Characteristics

The API layer is:
- Stateless
- Thin
- Predictable
- Secure
- Role-aware
- CQRS-based
- Easily testable

It serves as a clean boundary between the external world (client) and the internal Application layer.
