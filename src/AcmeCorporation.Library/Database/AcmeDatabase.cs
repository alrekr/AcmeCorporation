using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AcmeCorporation.Library.Database;

public class AcmeDatabase
{
    private readonly string? _connectionString;

    public AcmeDatabase(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AcmeConnectionString");
        
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("Connection string 'AcmeConnectionString' not found");
        }
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}