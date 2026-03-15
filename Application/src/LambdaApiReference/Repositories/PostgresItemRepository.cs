using System.Data;
using System.Diagnostics.CodeAnalysis;
using Dapper;
using LambdaApiReference.Models;

namespace LambdaApiReference.Repositories;

[ExcludeFromCodeCoverage]
public class PostgresItemRepository(IDbConnectionFactory dbConnectionFactory) : IItemRepository
{
    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        const string sql = """
            SELECT id, name, description, created_at, updated_at
            FROM   reference.items
            ORDER  BY created_at
            """;

        using IDbConnection conn = dbConnectionFactory.Create();
        return await conn.QueryAsync<Item>(sql);
    }

    public async Task<Item?> GetByIdAsync(string id)
    {
        const string sql = """
            SELECT id, name, description, created_at, updated_at
            FROM   reference.items
            WHERE  id = @Id
            """;

        using IDbConnection conn = dbConnectionFactory.Create();
        return await conn.QuerySingleOrDefaultAsync<Item>(sql, new { Id = id });
    }

    public async Task<Item> CreateAsync(CreateItemRequest request)
    {
        const string sql = """
            INSERT INTO reference.items (id, name, description, created_at)
            VALUES (@Id, @Name, @Description, @CreatedAt)
            RETURNING id, name, description, created_at, updated_at
            """;

        using IDbConnection conn = dbConnectionFactory.Create();
        return await conn.QuerySingleAsync<Item>(sql, new
        {
            Id          = Guid.NewGuid().ToString(),
            request.Name,
            request.Description,
            CreatedAt   = DateTime.UtcNow,
        });
    }

    public async Task<Item?> UpdateAsync(string id, UpdateItemRequest request)
    {
        const string sql = """
            UPDATE reference.items
            SET    name        = @Name,
                   description = @Description,
                   updated_at  = @UpdatedAt
            WHERE  id = @Id
            RETURNING id, name, description, created_at, updated_at
            """;

        using IDbConnection conn = dbConnectionFactory.Create();
        return await conn.QuerySingleOrDefaultAsync<Item>(sql, new
        {
            Id          = id,
            request.Name,
            request.Description,
            UpdatedAt   = DateTime.UtcNow,
        });
    }

    public async Task<bool> DeleteAsync(string id)
    {
        const string sql = "DELETE FROM reference.items WHERE id = @Id";

        using IDbConnection conn = dbConnectionFactory.Create();
        int rows = await conn.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}
