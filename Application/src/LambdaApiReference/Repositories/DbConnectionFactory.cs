using System.Data;
using System.Diagnostics.CodeAnalysis;
using Dapper;
using Npgsql;

namespace LambdaApiReference.Repositories;

public interface IDbConnectionFactory
{
    public IDbConnection Create();
}

[ExcludeFromCodeCoverage]
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        IConfigurationSection rds = configuration.GetSection("RDSConnection");

        _connectionString = new NpgsqlConnectionStringBuilder
        {
            Host     = rds["Server"],
            Port     = int.Parse(rds["Port"] ?? "5432"),
            Database = rds["Database"],
            Username = rds["Username"],
            Password = rds["Password"],
        }.ConnectionString;

        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public IDbConnection Create() => new NpgsqlConnection(_connectionString);
}
