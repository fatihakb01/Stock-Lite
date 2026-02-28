# Architecture Overview

This document describes the architectural structure of Stock-Lite, the relationships between its layers, and the design principles applied.

Stock-Lite follows principles inspired by Clean Architecture and SOLID, with clear separation of concerns and strict dependency rules. The goal is to keep the core business logic independent from technical concerns such as HTTP, databases, or frontend frameworks.

## 🏛 Architectural Structure

📦Stock-Lite  
 ┣ 📂API  
 ┣ 📂Application  
 ┣ 📂Domain  
 ┣ 📂Infrastructure  
 ┣ 📂client  
 ┗ 📂docs

The layers reference each other in the following way:
- API → Application & Infrastructure
- Application → Domain
- Infrastructure → Domain
- Domain → (no dependencies)

The Domain layer is fully independent and does not reference any other layer. This ensures that business rules remain isolated and reusable, and that infrastructure or framework changes do not affect the core logic.

## 📦 Layer Responsibilities

### 🌐 API Layer

The API layer is an ASP.NET Web API project responsible for:
- Exposing HTTP endpoints
- Routing incoming requests
- Executing commands or queries
- Returning structured responses

Controllers do not contain business logic.
Instead, they delegate work to the Application layer, executing a specific Command or Query depending on the endpoint being called.

The API acts purely as an orchestration and transport layer.

### 🧠 Application Layer

The Application layer contains:
- Commands & Queries
- Use case orchestration
- FluentValidation logic

This layer defines the workflows of the system. Each controller action triggers either a command (write operation) or a query (read operation).

#### Validation Strategy

Stock-Lite uses a two-layer validation approach:
- First line of defense → FluentValidation inside the Application layer
- Second line of defense → Database constraints

This ensures invalid data is caught early while still maintaining data integrity at the database level.

The Application layer depends only on the Domain layer.

### 💎 Domain Layer

The Domain layer is the core of the system and contains:
- Entity definitions
- Interfaces
- Enums
- Business rules

It is completely independent of all other layers.

#### Entity Categories

Entities are divided into two groups:
- Core Entities: these represent the fundamental building blocks required for the application to function (e.g., products, users, inventory policies).
- Derived Entities: these are calculated entities derived from core data. They represent outcomes of business calculations such as forecasting and stock recommendations.

The Domain layer defines interfaces for repositories and services.
Concrete implementations are provided by the Infrastructure layer.

### 🛠 Infrastructure Layer

The Infrastructure layer provides technical implementations and external integrations. It contains:
- Entity Framework configurations
- Repository implementations
- Service implementations
- Database context

#### ORM Configuration

All ORM configurations are centralized in Infrastructure.
This keeps the Domain layer free from persistence concerns.

#### Repository & Service Implementations

Infrastructure implements the interfaces defined in the Domain layer.
This ensures dependency inversion and supports testability.

#### Stored Procedures & Calculations

For derived entities and forecasting calculations:
- Stored procedures are defined inside database initializers.
- Repository classes execute these procedures.
- Results are mapped back to derived domain entities.

More detailed information about this can be found in [database.md](database.md).

### 🖥 Client (Frontend)

The client project is built using:
- TypeScript
- React

It is responsible for:
- Rendering the user interface
- Managing client-side interactions
- Communicating with the API via HTTP

The frontend contains no business logic related to forecasting or stock calculations — this logic resides in the backend.

## 🔄 Request & Data Flow Example
- A user performs an action in the React frontend.
- The client sends an HTTP request to the API.
- The API controller executes the corresponding Command or Query.
- The Application layer validates the request (FluentValidation).
- The Application layer coordinates domain logic.
- Infrastructure repositories retrieve or persist data.
- Stored procedures may be executed for calculation-heavy operations.

The result flows back through Application → API → Client.

## 📌 Related Documentation

For deeper technical details, see:
- [database.md](database.md) – Database structure, stored procedures, and migrations
- [api.md](api.md) – API endpoints and request/response structure
- [entities.md](entities.md) – Detailed entity descriptions