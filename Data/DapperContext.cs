using System.Data;
using Microsoft.Data.SqlClient;

namespace StoreApi.Data;

/// <summary>
/// Provides database connections for ADO.NET and Dapper repositories.
/// </summary>
public class DapperContext
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    /// <summary>
    /// Creates a new database connection for use with ADO.NET or Dapper.
    /// The caller is responsible for disposing the connection.
    /// </summary>
    public IDbConnection CreateConnection()
        => new SqlConnection(_connectionString);
}
