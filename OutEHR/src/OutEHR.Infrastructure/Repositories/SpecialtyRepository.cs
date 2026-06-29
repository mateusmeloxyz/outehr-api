using Dapper;
using OutEHR.Application.Interfaces;
using OutEHR.Domain.Entities;
using OutEHR.Infrastructure.Data;

namespace OutEHR.Infrastructure.Repositories;

public class SpecialtyRepository : BaseRepository<Specialty>, ISpecialtyRepository
{
    protected override string TableName => "Specialties";
    protected override string SelectColumns => "Id, Name, Description, DefaultSlotDurationMinutes, IsActive, CreatedAt, UpdatedAt";
    protected override string IdColumn => "Id";

    public SpecialtyRepository(DbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Specialty?> GetByNameAsync(string name)
    {
        using var connection = ConnectionFactory.CreateConnection();
        var sql = $"SELECT {SelectColumns} FROM {TableName} WHERE Name = @Name AND IsActive = 1";
        return await connection.QueryFirstOrDefaultAsync<Specialty>(sql, new { Name = name });
    }

    protected override string GetInsertSql()
    {
        return @"
            INSERT INTO Specialties (Name, Description, DefaultSlotDurationMinutes)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Description, @DefaultSlotDurationMinutes)";
    }

    protected override string GetUpdateSql()
    {
        return @"
            UPDATE Specialties
            SET Name = @Name,
                Description = @Description,
                DefaultSlotDurationMinutes = @DefaultSlotDurationMinutes,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id AND IsActive = 1";
    }
}
