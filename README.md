# StoreApi

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4?style=flat-square)](https://learn.microsoft.com/ef/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)](LICENSE)

A modern, production-ready RESTful API for e-commerce applications built with .NET 8. Features JWT authentication, role-based authorization, pagination, rate limiting, and structured logging.

## Overview

StoreApi provides a complete backend solution for managing an online store, including:

- **Product Management** ��� Full CRUD operations with category filtering and search
- **Order Processing** ��� Complete order lifecycle with payment tracking
- **User Management** ��� Registration, authentication, and role-based access
- **Category Management** ��� Organize products into categories

### Architecture

```
Controller ��� Service ��� Repository ��� DbContext (EF Core + SQL Server)
```

The API follows a clean layered architecture with clear separation of concerns:

| Layer | Responsibility |
|-------|----------------|
| Controllers | HTTP handling, validation, response formatting |
| Services | Business logic, DTO mapping |
| Repositories | Data access via Entity Framework Core |
| Models | Domain entities |

## Features

- **JWT Authentication** with configurable token expiration
- **Role-based Authorization** (`manager`, `customer`)
- **Pagination Support** for large datasets
- **Rate Limiting** to prevent API abuse
- **Structured Logging** with Serilog (console + file)
- **Swagger/OpenAPI** documentation
- **SQL Server** with Entity Framework Core migrations

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (or SQL Server Express/LocalDB)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/StoreApi.git
   cd StoreApi
   ```

2. **Configure the databxxxxxxxxxction**
   
   Update `appsettings.json` with your SQL Server connection string:
   ```json
   {
     "ConnectionStrings": {
       "Defaultxxxxxction": "Server=your-server;Database=storedb;Integrated Security=SSPI;TrustServerCertificate=True;"
     }
   }
   ```

3. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:5001` with Swagger UI at `/swagger`.

## API Endpoints

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Authenticate and get JWT token |
| POST | `/api/auth/register` | Register a new user |

### Products (Manager role required)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Get all products |
| GET | `/api/products/paged` | Get products with pagination |
| GET | `/api/products/{id}` | Get product by ID |
| GET | `/api/products/category/{categoryId}` | Get products by category |
| GET | `/api/products/search?name=` | Search products by name |
| POST | `/api/products` | Create a new product |
| PUT | `/api/products/{id}` | Update a product |
| DELETE | `/api/products/{id}` | Delete a product |

### Categories

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/categories` | Get all categories |
| GET | `/api/categories/{id}` | Get category by ID |
| POST | `/api/categories` | Create a new category |
| PUT | `/api/categories/{id}` | Update a category |
| DELETE | `/api/categories/{id}` | Delete a category |

### Orders

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/orders` | Get all orders |
| GET | `/api/orders/{id}` | Get order by ID |
| GET | `/api/orders/user/{userId}` | Get orders by user |
| POST | `/api/orders` | Create a new order |
| PUT | `/api/orders/{id}` | Update an order |
| DELETE | `/api/orders/{id}` | Delete an order |

### Users

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/users` | Get all users |
| GET | `/api/users/{id}` | Get user by ID |
| PUT | `/api/users/{id}` | Update a user |
| DELETE | `/api/users/{id}` | Delete a user |

## Configuration

Key settings in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "StoreApi",
    "Audience": "StoreApiUsers",
    "ExpiryMinutes": 60
  },
  "RateLimiting": {
    "RequestLimit": 100,
    "TimeWindowMinutes": 1
  }
}
```

> [!IMPORTANT]
> Never commit your JWT secret key to source control. Use [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or environment variables in production.

## Development

### Running Tests

```bash
dotnet test StoreApi.Tests/
```

The project includes:
- **Unit tests** with mocked repositories
- **Integration tests** with EF Core InMemory database

### Database Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Revert last migration
dotnet ef migrations remove
```

### Seed Data

To populate the database with sample data, execute the SQL script:

```bash
sqlcmd -S your-server -d storedb -i Scripts/SeedData.sql
```

## Project Structure

```
StoreApi/
��������� Controllers/     # API endpoints
��������� Services/        # Business logic
��������� Repositories/    # Data access layer
��������� Interfaces/      # Service and repository contracts
��������� Models/          # Domain entities
��������� DTOs/            # Data transfer objects
��������� Data/            # DbContext and configurations
��������� Middleware/      # Request logging, rate limiting
��������� Migrations/      # EF Core migrations
��������� Logs/            # Application logs
��������� Scripts/         # Database scripts
```

## Resources

- [ASP.NET Core Documentation](https://learn.microsoft.com/aspnet/core)
- [Entity Framework Core](https://learn.microsoft.com/ef/core)
- [JWT Authentication in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authentication)
- [Serilog Documentation](https://serilog.net/)

