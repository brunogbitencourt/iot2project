using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Iot2Project.Infrastructure.Persistence.Context;

public class PgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public PgsqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
