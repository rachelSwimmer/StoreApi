using System.Data;
using Microsoft.Data.SqlClient;
using StoreApi.Data;
using StoreApi.Interfaces;
using StoreApi.Models;

namespace StoreApi.Repositories;

/// <summary>
/// Product repository implementation using raw ADO.NET for data access.
/// Demonstrates direct SQL execution without ORM.
/// </summary>
public class ProductAdoRepository : IProductRepository
{
    private readonly DapperContext _context;

    public ProductAdoRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        var products = new List<Product>();

        using var connection = _context.CreateConnection();
        using var command = (SqlCommand)connection.CreateCommand();
        
        command.CommandText = @"
            SELECT p.Id, p.Name, p.Description, p.Price, p.Stock, p.CategoryId, 
                   p.CreatedAt, p.UpdatedAt, c.Id AS CatId, c.Name AS CatName, c.Description AS CatDescription
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            ORDER BY p.Id";

        connection.Open();
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(MapProductFromReader(reader));
        }

        return products;
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var products = new List<Product>();
        int totalCount = 0;

        using var connection = _context.CreateConnection();
        using var command = (SqlCommand)connection.CreateCommand();

        command.CommandText = @"
            SELECT COUNT(*) FROM Products;
            
            SELECT p.Id, p.Name, p.Description, p.Price, p.Stock, p.CategoryId, 
                   p.CreatedAt, p.UpdatedAt, c.Id AS CatId, c.Name AS CatName, c.Description AS CatDescription
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            ORDER BY p.Id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        connection.Open();

        using var reader = await command.ExecuteReaderAsync();
        
        // First result set: total count
        if (await reader.ReadAsync())
        {
            totalCount = reader.GetInt32(0);
        }

        // Second result set: paged products
        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            products.Add(MapProductFromReader(reader));
        }

        return (products, totalCount);
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        using var command = (SqlCommand)connection.CreateCommand();

        command.CommandText = @"
            SELECT p.Id, p.Name, p.Description, p.Price, p.Stock, p.CategoryId, 
                   p.CreatedAt, p.UpdatedAt, c.Id AS CatId, c.Name AS CatName, c.Description AS CatDescription
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            WHERE p.Id = @Id";

        command.Parameters.AddWithValue("@Id", id);

        connection.Open();

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapProductFromReader(reader);
        }

        return null;
    }

    public async Task<Product> CreateAsync(Product product)
    {
        using var connection = _context.CreateConnection();
        using var command = (SqlCommand)connection.CreateCommand();

        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        command.CommandText = @"
            INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt, UpdatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Description, @Price, @Stock, @CategoryId, @CreatedAt, @UpdatedAt)";

        command.Parameters.AddWithValue("@Name", product.Name);
        command.Parameters.AddWithValue("@Description", product.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@Stock", product.Stock);
        command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
        command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", product.UpdatedAt);

        connection.Open();

        var id = await command.ExecuteScalarAsync();
        product.Id = Convert.ToInt32(id);

        return product;
    }

    public async Task<Product?> UpdateAsync(Product product)
    {
        using var connection = _context.CreateConnection();
        using var command = (SqlCommand)connection.CreateCommand();

        product.UpdatedAt = DateTime.UtcNow;

        command.CommandText = @"
            UPDATE Products 
            SET Name = @Name, 
                Description = @Description, 
                Price = @Price, 
                Stock = @Stock, 
                CategoryId = @CategoryId, 
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        command.Parameters.AddWithValue("@Id", product.Id);
        command.Parameters.AddWithValue("@Name", product.Name);
        command.Parameters.AddWithValue("@Description", product.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@Stock", product.Stock);
        command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
        command.Parameters.AddWithValue("@UpdatedAt", product.UpdatedAt);

        connection.Open();

        var rowsAffected = await command.ExecuteNonQueryAsync();
        
        if (rowsAffected > 0)
        {
            return await GetByIdAsync(product.Id);
        }

        return null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _context.CreateConnection();
        using var command = (SqlCommand)connection.CreateCommand();

        command.CommandText = "DELETE FROM Products WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        connection.Open();

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = _context.CreateConnection();
        using var command = (SqlCommand)connection.CreateCommand();

        command.CommandText = "SELECT COUNT(1) FROM Products WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        connection.Open();

        var count = await command.ExecuteScalarAsync();
        return Convert.ToInt32(count) > 0;
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        var products = new List<Product>();

        using var connection = _context.CreateConnection();
        using var command = (SqlCommand)connection.CreateCommand();

        command.CommandText = @"
            SELECT p.Id, p.Name, p.Description, p.Price, p.Stock, p.CategoryId, 
                   p.CreatedAt, p.UpdatedAt, c.Id AS CatId, c.Name AS CatName, c.Description AS CatDescription
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            WHERE p.CategoryId = @CategoryId
            ORDER BY p.Id";

        command.Parameters.AddWithValue("@CategoryId", categoryId);

        connection.Open();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(MapProductFromReader(reader));
        }

        return products;
    }

    public async Task<IEnumerable<Product>> SearchByNameAsync(string searchTerm)
    {
        var products = new List<Product>();

        using var connection = _context.CreateConnection();
        using var command = (SqlCommand)connection.CreateCommand();

        command.CommandText = @"
            SELECT p.Id, p.Name, p.Description, p.Price, p.Stock, p.CategoryId, 
                   p.CreatedAt, p.UpdatedAt, c.Id AS CatId, c.Name AS CatName, c.Description AS CatDescription
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            WHERE p.Name LIKE @SearchTerm
            ORDER BY p.Id";

        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

        connection.Open();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(MapProductFromReader(reader));
        }

        return products;
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> SearchByNamePagedAsync(
        string searchTerm, int pageNumber, int pageSize)
    {
        var products = new List<Product>();
        int totalCount = 0;

        using var connection = _context.CreateConnection();
        using var command = (SqlCommand)connection.CreateCommand();

        command.CommandText = @"
            SELECT COUNT(*) FROM Products WHERE Name LIKE @SearchTerm;
            
            SELECT p.Id, p.Name, p.Description, p.Price, p.Stock, p.CategoryId, 
                   p.CreatedAt, p.UpdatedAt, c.Id AS CatId, c.Name AS CatName, c.Description AS CatDescription
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            WHERE p.Name LIKE @SearchTerm
            ORDER BY p.Id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
        command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        connection.Open();

        using var reader = await command.ExecuteReaderAsync();

        // First result set: total count
        if (await reader.ReadAsync())
        {
            totalCount = reader.GetInt32(0);
        }

        // Second result set: paged products
        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            products.Add(MapProductFromReader(reader));
        }

        return (products, totalCount);
    }

    private static Product MapProductFromReader(SqlDataReader reader)
    {
        var product = new Product
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) 
                ? string.Empty 
                : reader.GetString(reader.GetOrdinal("Description")),
            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
            Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
            CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };

        // Map Category if available
        if (!reader.IsDBNull(reader.GetOrdinal("CatId")))
        {
            product.Category = new Category
            {
                Id = reader.GetInt32(reader.GetOrdinal("CatId")),
                Name = reader.GetString(reader.GetOrdinal("CatName")),
                Description = reader.IsDBNull(reader.GetOrdinal("CatDescription"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("CatDescription"))
            };
        }

        return product;
    }
}
