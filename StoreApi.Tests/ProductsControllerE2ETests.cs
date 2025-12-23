using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreApi.Data;
using StoreApi.DTOs;
using StoreApi.Models;

namespace StoreApi.Tests;

/// <summary>
/// End-to-End (E2E) tests that test the full application stack:
/// HTTP Request -> Controller -> Service -> Repository -> Database -> HTTP Response
/// Uses WebApplicationFactory to spin up a real test server
/// </summary>
public class ProductsControllerE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public ProductsControllerE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("E2ETestDb");
                });

                // Build service provider and seed database
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                db.Database.EnsureCreated();
                SeedDatabase(db);
            });
        });

        _client = _factory.CreateClient();
    }

    private void SeedDatabase(ApplicationDbContext context)
    {
        // Clear existing data
        context.Products.RemoveRange(context.Products);
        context.Categories.RemoveRange(context.Categories);
        context.SaveChanges();

        // Seed test data
        var category = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices",
            CreatedAt = DateTime.UtcNow
        };

        context.Categories.Add(category);

        var products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Laptop",
                Description = "Gaming laptop",
                Price = 999.99m,
                Stock = 10,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "Mouse",
                Description = "Wireless mouse",
                Price = 29.99m,
                Stock = 50,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }

    [Fact]
    public async Task GET_AllProducts_ReturnsSuccessAndProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<List<ProductResponseDto>>();
        Assert.NotNull(products);
        Assert.Equal(2, products.Count);
        Assert.Contains(products, p => p.Name == "Laptop");
    }

    [Fact]
    public async Task GET_ProductById_WithValidId_ReturnsProduct()
    {
        // Act
        var response = await _client.GetAsync("/api/products/1");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
        Assert.NotNull(product);
        Assert.Equal("Laptop", product.Name);
        Assert.Equal(999.99m, product.Price);
        Assert.Equal("Electronics", product.CategoryName);
    }

    [Fact]
    public async Task GET_ProductById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/products/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task POST_CreateProduct_WithValidData_ReturnsCreated()
    {
        // Arrange
        var newProduct = new ProductCreateDto
        {
            Name = "Keyboard",
            Description = "Mechanical keyboard",
            Price = 89.99m,
            Stock = 25,
            CategoryId = 1
        };

        var content = new StringContent(
            JsonSerializer.Serialize(newProduct),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdProduct = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
        Assert.NotNull(createdProduct);
        Assert.Equal("Keyboard", createdProduct.Name);
        Assert.Equal(89.99m, createdProduct.Price);

        // Verify Location header
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task POST_CreateProduct_WithInvalidCategory_ReturnsBadRequest()
    {
        // Arrange
        var invalidProduct = new ProductCreateDto
        {
            Name = "Invalid Product",
            Description = "This should fail",
            Price = 50.00m,
            Stock = 5,
            CategoryId = 999 // Non-existent category
        };

        var content = new StringContent(
            JsonSerializer.Serialize(invalidProduct),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errorResponse = await response.Content.ReadAsStringAsync();
        Assert.Contains("Category with ID 999 does not exist", errorResponse);
    }

    [Fact]
    public async Task POST_CreateProduct_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange - Missing required fields
        var invalidProduct = new
        {
            Name = "", // Empty name (invalid)
            Price = -10m, // Negative price (invalid)
            CategoryId = 1
        };

        var content = new StringContent(
            JsonSerializer.Serialize(invalidProduct),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PUT_UpdateProduct_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var updateDto = new ProductUpdateDto
        {
            Name = "Updated Laptop",
            Price = 1099.99m,
            Stock = 15
        };

        var content = new StringContent(
            JsonSerializer.Serialize(updateDto),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PutAsync("/api/products/1", content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedProduct = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
        Assert.NotNull(updatedProduct);
        Assert.Equal("Updated Laptop", updatedProduct.Name);
        Assert.Equal(1099.99m, updatedProduct.Price);
    }

    [Fact]
    public async Task PUT_UpdateProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new ProductUpdateDto
        {
            Name = "Should Not Update"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(updateDto),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PutAsync("/api/products/999", content);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DELETE_Product_WithValidId_ReturnsNoContent()
    {
        // Act
        var response = await _client.DeleteAsync("/api/products/2");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify product is deleted
        var getResponse = await _client.GetAsync("/api/products/2");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DELETE_Product_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/products/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CompleteProductLifecycle_CreateReadUpdateDelete_WorksEndToEnd()
    {
        // 1. CREATE
        var createDto = new ProductCreateDto
        {
            Name = "Headphones",
            Description = "Noise-cancelling headphones",
            Price = 199.99m,
            Stock = 20,
            CategoryId = 1
        };

        var createContent = new StringContent(
            JsonSerializer.Serialize(createDto),
            Encoding.UTF8,
            "application/json"
        );

        var createResponse = await _client.PostAsync("/api/products", createContent);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();
        Assert.NotNull(createdProduct);
        var productId = createdProduct.Id;

        // 2. READ
        var getResponse = await _client.GetAsync($"/api/products/{productId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var retrievedProduct = await getResponse.Content.ReadFromJsonAsync<ProductResponseDto>();
        Assert.Equal("Headphones", retrievedProduct!.Name);
        Assert.Equal(199.99m, retrievedProduct.Price);

        // 3. UPDATE
        var updateDto = new ProductUpdateDto
        {
            Name = "Premium Headphones",
            Price = 249.99m
        };

        var updateContent = new StringContent(
            JsonSerializer.Serialize(updateDto),
            Encoding.UTF8,
            "application/json"
        );

        var updateResponse = await _client.PutAsync($"/api/products/{productId}", updateContent);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedProduct = await updateResponse.Content.ReadFromJsonAsync<ProductResponseDto>();
        Assert.Equal("Premium Headphones", updatedProduct!.Name);
        Assert.Equal(249.99m, updatedProduct.Price);

        // 4. DELETE
        var deleteResponse = await _client.DeleteAsync($"/api/products/{productId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // 5. VERIFY DELETED
        var verifyResponse = await _client.GetAsync($"/api/products/{productId}");
        Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
    }

    [Fact]
    public async Task GET_Products_AfterMultipleOperations_ReturnsCorrectData()
    {
        // Create a new product
        var newProduct = new ProductCreateDto
        {
            Name = "Monitor",
            Description = "4K Monitor",
            Price = 399.99m,
            Stock = 15,
            CategoryId = 1
        };

        var content = new StringContent(
            JsonSerializer.Serialize(newProduct),
            Encoding.UTF8,
            "application/json"
        );

        await _client.PostAsync("/api/products", content);

        // Get all products
        var response = await _client.GetAsync("/api/products");
        var products = await response.Content.ReadFromJsonAsync<List<ProductResponseDto>>();

        // Should have original 2 + 1 new = 3 products
        Assert.NotNull(products);
        Assert.Contains(products, p => p.Name == "Monitor");
    }
}
