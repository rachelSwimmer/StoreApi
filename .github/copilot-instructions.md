# StoreApi - AI Coding Agent Instructions

## Architecture Overview

This is a **.NET 8 Web API** for an e-commerce store using a **layered architecture**:

```
Controller → Service → Repository → DbContext (EF Core + SQL Server)
```

**Data flows as:** `HTTP Request → Controller (DTOs) → Service (business logic) → Repository (data access) → Models`

### Key Design Decisions
- **DTOs separate API contracts from domain models** - never expose `Models/` directly in responses
- **Services own business logic and DTO mapping** - repositories return domain models only
- **Interfaces live in `/Interfaces`** for DI and testability
- **JWT authentication** with role-based authorization (`manager`, `customer`)

## Project Structure

| Folder | Purpose |
|--------|---------|
| `Controllers/` | Thin HTTP handlers - delegate to services |
| `Services/` | Business logic + DTO↔Model mapping |
| `Repositories/` | EF Core data access - return `Models` |
| `DTOs/` | API contracts: `{Entity}CreateDto`, `{Entity}UpdateDto`, `{Entity}ResponseDto` |
| `Models/` | EF Core entities with navigation properties |
| `Middleware/` | Request logging, rate limiting |
| `Data/` | `ApplicationDbContext` with Fluent API configs |
|

## Critical Patterns


### DTO Naming Convention
```csharp
ProductCreateDto   // POST body
ProductUpdateDto   // PUT body (nullable properties for partial updates)
ProductResponseDto // All responses
PaginationParams   // Query params for paged endpoints
PagedResult<T>     // Paged response wrapper

```

### Service Method Pattern
Services map DTOs to models and vice versa. See [ProductService.cs](Services/ProductService.cs):
```csharp
public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto createDto)
{
    // 1. Validate business rules (e.g., check category exists)
    // 2. Map DTO → Model
    // 3. Call repository
    // 4. Map Model → ResponseDto
}
```

### Repository Pattern
Repositories use `Include()` for eager loading navigation properties:
```csharp
return await _context.Products
    .Include(p => p.Category)
    .FirstOrDefaultAsync(p => p.Id == id);
```

### Pagination Pattern
Use `PaginationParams` for paged endpoints. Return `PagedResult<T>`:
```csharp
[HttpGet("paged")]
public async Task<ActionResult<PagedResult<ProductResponseDto>>> GetAllPaged([FromQuery] PaginationParams paginationParams)
```

## Authentication & Authorization

- **JWT tokens** configured in `appsettings.json` under `JwtSettings`
- **User types**: `manager` (full access), `customer` (limited access)
- `TokenService.GenerateToken()` includes `ClaimTypes.Role` for authorization
- Use `[Authorize(Roles = "manager")]` on protected endpoints

## Developer Workflows

### Run the API
```bash
dotnet run
```
API available at `https://localhost:5001` with Swagger UI.

### Database Migrations
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Run Tests
```bash
dotnet test StoreApi.Tests/
```
- **Unit tests**: Mock repositories with Moq (see [ProductServiceTests.cs](StoreApi.Tests/ProductServiceTests.cs))
- **Integration tests**: Use EF Core InMemory database (see [ProductServiceIntegrationTests.cs](StoreApi.Tests/ProductServiceIntegrationTests.cs))

### Seed Database
Execute [Scripts/SeedData.sql](Scripts/SeedData.sql) against SQL Server for sample data.

## Configuration

Key settings in `appsettings.json`:
- `ConnectionStrings:DefaultConnection` - SQL Server connection
- `JwtSettings` - Token generation (SecretKey, Issuer, Audience, ExpiryMinutes)
- `RateLimiting` - RequestLimit and TimeWindowMinutes
- `Serilog` - Structured logging to console and `Logs/` folder

## Adding New Entities

1. Create model in `Models/` with navigation properties
2. Add `DbSet<T>` to `ApplicationDbContext` with Fluent API config
3. Create DTOs in `DTOs/`: `{Entity}CreateDto`, `{Entity}UpdateDto`, `{Entity}ResponseDto`
4. Create interface in `Interfaces/`: `I{Entity}Repository`, `I{Entity}Service`
5. Implement repository in `Repositories/`
6. Implement service in `Services/` with DTO mapping
7. Create controller in `Controllers/` with `[ApiController]`, `[Route("api/[controller]")]`
8. Register in `Program.cs` DI container


## Conventions

- **Async/await everywhere** - all data operations are async
- **Logging**: Use injected `ILogger<T>`, structured logging with Serilog
- **Error responses**: Return `new { message = "..." }` for consistency
- **Timestamps**: Use `DateTime.UtcNow` for `CreatedAt`/`UpdatedAt`
- **Partial updates**: `UpdateDto` properties are nullable; only update non-null values
- dont write any comments in the code, only write code.
