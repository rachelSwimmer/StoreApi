using Xunit;
using Moq;
using StoreApi.Services;
using StoreApi.Interfaces;
using StoreApi.Models;
using StoreApi.DTOs;
using Microsoft.Extensions.Logging;

namespace StoreApi.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task GetAllProductsAsync_ReturnsListOfProducts()
    {
        // Arrange
        var mockProductRepo = new Mock<IProductRepository>();
        var mockCategoryRepo = new Mock<ICategoryRepository>();
        var mockLogger = new Mock<ILogger<ProductService>>();

        var products = new List<Product>
        {
            new Product 
            { 
                Id = 1, 
                Name = "Product 1", 
                Price = 10.99m,
                Stock = 5,
                CategoryId = 1,
                Category = new Category { Id = 1, Name = "Category 1" }
            },
            new Product 
            { 
                Id = 2, 
                Name = "Product 2", 
                Price = 20.99m,
                Stock = 10,
                CategoryId = 1,
                Category = new Category { Id = 1, Name = "Category 1" }
            }
        };

        mockProductRepo.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(products);

        var service = new ProductService(mockProductRepo.Object, mockCategoryRepo.Object, mockLogger.Object);

        // Act
        var result = await service.GetAllProductsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        mockProductRepo.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var mockProductRepo = new Mock<IProductRepository>();
        var mockCategoryRepo = new Mock<ICategoryRepository>();
        var mockLogger = new Mock<ILogger<ProductService>>();

        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            Price = 15.99m,
            Stock = 3,
            CategoryId = 1,
            Category = new Category { Id = 1, Name = "Test Category" }
        };

        mockProductRepo.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(product);

        var service = new ProductService(mockProductRepo.Object, mockCategoryRepo.Object, mockLogger.Object);

        // Act
        var result = await service.GetProductByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(15.99m, result.Price);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var mockProductRepo = new Mock<IProductRepository>();
        var mockCategoryRepo = new Mock<ICategoryRepository>();
        var mockLogger = new Mock<ILogger<ProductService>>();

        mockProductRepo.Setup(repo => repo.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(mockProductRepo.Object, mockCategoryRepo.Object, mockLogger.Object);

        // Act
        var result = await service.GetProductByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateProductAsync_WithInvalidCategory_ThrowsArgumentException()
    {
        // Arrange
        var mockProductRepo = new Mock<IProductRepository>();
        var mockCategoryRepo = new Mock<ICategoryRepository>();
        var mockLogger = new Mock<ILogger<ProductService>>();

        var createDto = new ProductCreateDto
        {
            Name = "New Product",
            Price = 25.99m,
            Stock = 7,
            CategoryId = 999 // Invalid category
        };

        mockCategoryRepo.Setup(repo => repo.ExistsAsync(999))
            .ReturnsAsync(false);

        var service = new ProductService(mockProductRepo.Object, mockCategoryRepo.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProductAsync(createDto));
    }
}
