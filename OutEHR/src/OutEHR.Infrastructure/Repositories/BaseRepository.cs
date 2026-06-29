using Dapper;
using OutEHR.Application.DTOs.Common;
using OutEHR.Application.Interfaces;
using OutEHR.Infrastructure.Data;

namespace OutEHR.Infrastructure.Repositories;

public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly DbConnectionFactory ConnectionFactory;
    protected abstract string TableName { get; }
    protected abstract string SelectColumns { get; }
    protected abstract string IdColumn { get; }

    protected BaseRepository(DbConnectionFactory connectionFactory)
    {
        ConnectionFactory = connectionFactory;
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        using var connection = ConnectionFactory.CreateConnection();
        var sql = $"SELECT {SelectColumns} FROM {TableName} WHERE {IdColumn} = @Id AND IsActive = 1";
        return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    public async Task<PagedResult<T>> GetAllAsync(int page, int pageSize)
    {
        using var connection = ConnectionFactory.CreateConnection();

        var offset = (page - 1) * pageSize;
        var countSql = $"SELECT COUNT(*) FROM {TableName} WHERE IsActive = 1";
        var dataSql = $@"
            SELECT {SelectColumns}
            FROM {TableName}
            WHERE IsActive = 1
            ORDER BY {IdColumn}
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
        var data = await connection.QueryAsync<T>(dataSql, new { Offset = offset, PageSize = pageSize });

        return new PagedResult<T>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<T> AddAsync(T entity)
    {
        using var connection = ConnectionFactory.CreateConnection();
        var id = await connection.ExecuteScalarAsync<int>(GetInsertSql(), entity);
        return await GetByIdAsync(id) ?? entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(GetUpdateSql(), entity);
        return entity;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        using var connection = ConnectionFactory.CreateConnection();
        var sql = $"UPDATE {TableName} SET IsActive = 0, UpdatedAt = GETUTCDATE() WHERE {IdColumn} = @Id AND IsActive = 1";
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = ConnectionFactory.CreateConnection();
        var sql = $"SELECT COUNT(1) FROM {TableName} WHERE {IdColumn} = @Id AND IsActive = 1";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    protected abstract string GetInsertSql();
    protected abstract string GetUpdateSql();
}
