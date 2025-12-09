---
applyTo: '**'
---

# .NET API Architecture Guidelines

## Project Structure

Organize the project with the following folder structure:
```
/Models          - Domain entities and business models
/DTOs            - Data Transfer Objects for API requests/responses
/Repositories    - Data access layer interfaces and implementations
/Services        - Business logic layer interfaces and implementations
/Controllers     - API endpoints and HTTP handling
/Data            - DbContext and database configurations
/Interfaces      - Shared interfaces (can be organized by concern)
```

## 1. Models (Domain Entities)

**Purpose**: Represent database entities and core business objects.

**Guidelines**:
- Place in `Models/` folder
- Use PascalCase for class and property names
- Include data annotations for EF Core configuration
- Keep models focused on data structure, not behavior
- Use navigation properties for relationships

**Example**:
```csharp
namespace StoreApi.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Category? Category { get; set; }
}
```

## 2. DTOs (Data Transfer Objects)

**Purpose**: Define the shape of data sent to/from the API, separate from domain models.

**Guidelines**:
- Place in `DTOs/` folder
- Create separate DTOs for different operations: `CreateDto`, `UpdateDto`, `ResponseDto`
- Use data annotations for validation (`[Required]`, `[MaxLength]`, etc.)
- Never expose domain models directly through APIs
- Keep DTOs flat and simple
- Use nullable types appropriately

**Naming Convention**:
- `{Entity}CreateDto` - for POST requests
- `{Entity}UpdateDto` - for PUT/PATCH requests
- `{Entity}ResponseDto` or `{Entity}Dto` - for responses
- `{Entity}QueryDto` - for query parameters

**Example**:
```csharp
namespace StoreApi.DTOs;

public class ProductCreateDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
}

public class ProductUpdateDto
{
    [MaxLength(200)]
    public string? Name { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? Stock { get; set; }
    
    public int? CategoryId { get; set; }
}

public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

## 3. Repositories

**Purpose**: Handle all data access logic and database operations.

**Guidelines**:
- Create an interface in `Interfaces/` or `Repositories/` folder
- Implement the interface in `Repositories/` folder
- Use async/await for all database operations
- Return domain models, not DTOs
- Keep repository methods focused on data access only
- Use generic repository pattern for common CRUD operations
- Create specific repositories for complex queries

**Naming Convention**:
- Interface: `I{Entity}Repository`
- Implementation: `{Entity}Repository`

**Example**:
```csharp
// Interfaces/IProductRepository.cs
namespace StoreApi.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
}

// Repositories/ProductRepository.cs
namespace StoreApi.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    
    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .ToListAsync();
    }
    
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    public async Task<Product> CreateAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }
    
    public async Task<Product?> UpdateAsync(Product product)
    {
        var existing = await _context.Products.FindAsync(product.Id);
        if (existing == null) return null;
        
        _context.Entry(existing).CurrentValues.SetValues(product);
        existing.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return existing;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;
        
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(p => p.Id == id);
    }
    
    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Category)
            .ToListAsync();
    }
}
```

## 4. Services

**Purpose**: Implement business logic, orchestrate operations, and coordinate between repositories.

**Guidelines**:
- Create an interface in `Interfaces/` or `Services/` folder
- Implement the interface in `Services/` folder
- Use async/await for all operations
- Handle business validation and rules
- Map between DTOs and models
- Services should work with repositories, not DbContext directly
- Return DTOs or result objects, not domain models
- Handle exceptions and return appropriate results

**Naming Convention**:
- Interface: `I{Entity}Service`
- Implementation: `{Entity}Service`

**Example**:
```csharp
// Interfaces/IProductService.cs
namespace StoreApi.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync();
    Task<ProductResponseDto?> GetProductByIdAsync(int id);
    Task<ProductResponseDto> CreateProductAsync(ProductCreateDto createDto);
    Task<ProductResponseDto?> UpdateProductAsync(int id, ProductUpdateDto updateDto);
    Task<bool> DeleteProductAsync(int id);
}

// Services/ProductService.cs
namespace StoreApi.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<ProductService> _logger;
    
    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }
    
    public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToResponseDto);
    }
    
    public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product != null ? MapToResponseDto(product) : null;
    }
    
    public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto createDto)
    {
        // Business validation
        if (!await _categoryRepository.ExistsAsync(createDto.CategoryId))
        {
            throw new ArgumentException($"Category with ID {createDto.CategoryId} does not exist.");
        }
        
        var product = new Product
        {
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            Stock = createDto.Stock,
            CategoryId = createDto.CategoryId
        };
        
        var createdProduct = await _productRepository.CreateAsync(product);
        _logger.LogInformation("Product created with ID: {ProductId}", createdProduct.Id);
        
        return MapToResponseDto(createdProduct);
    }
    
    public async Task<ProductResponseDto?> UpdateProductAsync(int id, ProductUpdateDto updateDto)
    {
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null) return null;
        
        // Apply updates only for non-null properties
        if (updateDto.Name != null) existingProduct.Name = updateDto.Name;
        if (updateDto.Description != null) existingProduct.Description = updateDto.Description;
        if (updateDto.Price.HasValue) existingProduct.Price = updateDto.Price.Value;
        if (updateDto.Stock.HasValue) existingProduct.Stock = updateDto.Stock.Value;
        if (updateDto.CategoryId.HasValue)
        {
            if (!await _categoryRepository.ExistsAsync(updateDto.CategoryId.Value))
            {
                throw new ArgumentException($"Category with ID {updateDto.CategoryId} does not exist.");
            }
            existingProduct.CategoryId = updateDto.CategoryId.Value;
        }
        
        var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
        return updatedProduct != null ? MapToResponseDto(updatedProduct) : null;
    }
    
    public async Task<bool> DeleteProductAsync(int id)
    {
        return await _productRepository.DeleteAsync(id);
    }
    
    private static ProductResponseDto MapToResponseDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CategoryName = product.Category?.Name ?? string.Empty,
            CreatedAt = product.CreatedAt
        };
    }
}
```

## 5. Controllers

**Purpose**: Handle HTTP requests, validation, and return appropriate responses.

**Guidelines**:
- Place in `Controllers/` folder
- Inherit from `ControllerBase` for APIs (not `Controller`)
- Use `[ApiController]` attribute
- Use `[Route("api/[controller]")]` for RESTful routing
- Keep controllers thin - delegate to services
- Use appropriate HTTP status codes
- Handle exceptions with proper error responses
- Use async action methods
- Apply validation attributes

**Example**:
```csharp
namespace StoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;
    
    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponseDto>> GetById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found." });
        }
        
        return Ok(product);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponseDto>> Create([FromBody] ProductCreateDto createDto)
    {
        try
        {
            var product = await _productService.CreateProductAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponseDto>> Update(int id, [FromBody] ProductUpdateDto updateDto)
    {
        try
        {
            var product = await _productService.UpdateProductAsync(id, updateDto);
            
            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }
            
            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        
        if (!result)
        {
            return NotFound(new { message = $"Product with ID {id} not found." });
        }
        
        return NoContent();
    }
}
```

## 6. Dependency Injection (DI)

**Purpose**: Configure and manage application dependencies.

**Guidelines**:
- Register all services in `Program.cs`
- Use appropriate lifetimes:
  - **Scoped**: Repositories, Services, DbContext (per request)
  - **Transient**: Lightweight stateless services
  - **Singleton**: Configuration, caching, logging
- Register interfaces with their implementations
- Keep DI configuration organized and readable
- Use extension methods for complex registrations

**Example `Program.cs`**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repositories (Scoped - one instance per request)
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// Register Services (Scoped - one instance per request)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Configure JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## 7. Best Practices

### General
- Use async/await throughout the stack
- Implement proper error handling and logging
- Follow RESTful conventions for API endpoints
- Use meaningful and consistent naming conventions
- Keep methods focused and single-purpose
- Add XML documentation comments for public APIs

### DTOs and Mapping
- Consider using AutoMapper for complex mappings
- Never expose database entities directly
- Validate all input at the DTO level
- Use separate DTOs for different operations

### Repository Pattern
- Keep repositories focused on data access
- Don't implement business logic in repositories
- Use Include/ThenInclude for eager loading
- Consider implementing Unit of Work for transactions

### Service Layer
- Implement all business logic in services
- Services should orchestrate multiple repositories
- Handle cross-cutting concerns (logging, validation)
- Return DTOs or result objects, not entities

### Controllers
- Keep controllers thin and focused on HTTP concerns
- Delegate all logic to services
- Return appropriate HTTP status codes
- Use action filters for cross-cutting concerns

### Dependency Injection
- Prefer constructor injection
- Avoid service locator pattern
- Register dependencies with appropriate lifetimes
- Use interfaces for testability

## 8. Error Handling

**Implement global exception handling**:
```csharp
// Middleware/ExceptionMiddleware.cs
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
        
        return context.Response.WriteAsJsonAsync(new
        {
            statusCode = context.Response.StatusCode,
            message = exception.Message
        });
    }
}

// Register in Program.cs
app.UseMiddleware<ExceptionMiddleware>();
```

## 9. Testing Considerations

- Design for testability with interfaces
- Use dependency injection for easy mocking
- Keep business logic in services for unit testing
- Repositories should be easily mockable
- Controllers should be thin for easier testing

---

**Remember**: This architecture promotes separation of concerns, maintainability, and testability. Always follow the principle of least privilege and keep each layer focused on its specific responsibility.