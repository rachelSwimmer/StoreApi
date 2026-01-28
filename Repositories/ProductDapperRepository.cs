using Dapper;
using StoreApi.Data;
using StoreApi.Interfaces;
using StoreApi.Models;

namespace StoreApi.Repositories;

/// <summary>
/// Product repository implementation using Dapper micro-ORM.
/// Provides a cleaner API than raw ADO.NET while maintaining high performance.
/// </summary>
public class ProductDapperRepository : IProductRepository
{
    private readonly DapperContext _context;

    public ProductDapperRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        const string sql = @"
            SELECT p.Id, p.Name, p.Description, p.Price, p.Stock, p.CategoryId, p.CreatedAt, p.UpdatedAt,
                   c.Id, c.Name, c.Description, c.CreatedAt
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            ORDER BY p.Id";

        using var connection = _context.CreateConnection();

        var products = await connection.QueryAsync<Product, Category, Product>(
            sql,
            (product, category) =>
            {
                product.Category = category;
                return product;
            },
            splitOn: "Id");

        return products;
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        const string sql = @"
            SELECT COUNT(*) FROM Products;
            
            SELECT p.Id, p.Name, p.Description, p.Price, p.Stock, p.CategoryId, p.CreatedAt, p.UpdatedAt,
                   c.Id, c.Name, c.Description, c.CreatedAt
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            ORDER BY p.Id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var connection = _context.CreateConnection();

        using var multi = await connection.QueryMultipleAsync(sql, new
        {
            Offset = (pageNumber - 1) * pageSize,
            PageSize = pageSize
        });

        var totalCount = await multi.ReadSingleAsync<int>();
        
        var products = multi.Read<Product, Category, Product>(
            (product, category) =>
            {
                product.Category = category;
                return product;
            },
            splitOn: "Id");

        return (products.ToList(), totalCount);
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT p.Id, p.Name, p.Description, p.Price, p.Stock, p.CategoryId, p.CreatedAt, p.UpdatedAt,
                   c.Id, c.Name, c.Description, c.CreatedAt
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            WHERE p.Id = @Id";

        using var connection = _context.CreateConnection();

        var products = await connection.QueryAsync<Product, Category, Product>(
            sql,
            (product, category) =>
            {
                product.Category = category;
                return product;
            },
            new { Id = id },
            splitOn: "Id");

        return products.FirstOrDefault();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        const string sql = @"
            INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt, UpdatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Description, @Price, @Stock, @CategoryId, @CreatedAt, @UpdatedAt)";

        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        using var connection = _context.CreateConnection();

        var id = await connection.ExecuteScalarAsync<int>(sql, new
        {
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CategoryId,
            product.CreatedAt,
            product.UpdatedAt
        });

        product.Id = id;
        return product;
    }

    public async Task<Product?> UpdateAsync(Product product)
    {
        const string sql = @"
            UPDATE Products 
            SET Name = @Name, 
                Description = @Description, 
                Price = @Price, 
                Stock = @Stock, 
                CategoryId = @CategoryId, 
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        product.UpdatedAt = DateTime.UtcNow;

        using var connection = _context.CreateConnection();

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CategoryId,
            product.UpdatedAt
        });

        if (rowsAffected > 0)
        {
            return await GetByIdAsync(product.Id);
        }

        return null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Products WHERE Id = @Id";

        using var connection = _context.CreateConnection();

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM Products WHERE Id = @Id";

        using var connection = _context.CreateConnection();

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        const string sql = @"
            SELECT p.Id, p.Name, p.Description, p.Price, p.Stock, p.CategoryId, p.CreatedAt, p.UpdatedAt,
                   c.Id, c.Name, c.Description, c.CreatedAt
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            WHERE p.CategoryId = @CategoryId
            ORDER BY p.Id";

        using var connection = _context.CreateConnection();

        var products = await connection.QueryAsync<Product, Category, Product>(
            sql,
            (product, category) =>
            {
                product.Category = category;
                return product;
            },
            new { CategoryId = categoryId },
            splitOn: "Id");

        return products;
    }

    public async Task<IEnumerable<Product>> SearchByNameAsync(string searchTerm)
    {
        const string sql = @"
            SELECT p.Id, p.Name, p.Description, p.Price, p.Stock, p.CategoryId, p.CreatedAt, p.UpdatedAt,
                   c.Id, c.Name, c.Description, c.CreatedAt
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            WHERE p.Name LIKE @SearchTerm
            ORDER BY p.Id";

        using var connection = _context.CreateConnection();

        var products = await connection.QueryAsync<Product, Category, Product>(
            sql,
            (product, category) =>
            {
                product.Category = category;
                return product;
            },
            new { SearchTerm = $"%{searchTerm}%" },
            splitOn: "Id");

        return products;
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> SearchByNamePagedAsync(
        string searchTerm, int pageNumber, int pageSize)
    {
        const string sql = @"
            SELECT COUNT(*) FROM Products WHERE Name LIKE @SearchTerm;
            
            SELECT p.Id, p.Name, p.Description, p.Price, p.Stock, p.CategoryId, p.CreatedAt, p.UpdatedAt,
                   c.Id, c.Name, c.Description, c.CreatedAt
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            WHERE p.Name LIKE @SearchTerm
            ORDER BY p.Id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var connection = _context.CreateConnection();

        using var multi = await connection.QueryMultipleAsync(sql, new
        {
            SearchTerm = $"%{searchTerm}%",
            Offset = (pageNumber - 1) * pageSize,
            PageSize = pageSize
        });

        var totalCount = await multi.ReadSingleAsync<int>();
        
        var products = multi.Read<Product, Category, Product>(
            (product, category) =>
            {
                product.Category = category;
                return product;
            },
            splitOn: "Id");

        return (products.ToList(), totalCount);
    }
}
