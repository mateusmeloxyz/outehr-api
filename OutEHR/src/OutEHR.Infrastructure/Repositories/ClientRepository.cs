using Dapper;
using OutEHR.Application.Interfaces;
using OutEHR.Domain.Entities;
using OutEHR.Infrastructure.Data;

namespace OutEHR.Infrastructure.Repositories;

public class ClientRepository : BaseRepository<Client>, IClientRepository
{
    protected override string TableName => "Clients";
    protected override string SelectColumns => "Id, FirstName, LastName, Email, Phone, DateOfBirth, IsActive, CreatedAt, UpdatedAt";
    protected override string IdColumn => "Id";

    public ClientRepository(DbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Client?> GetByEmailAsync(string email)
    {
        using var connection = ConnectionFactory.CreateConnection();
        var sql = $"SELECT {SelectColumns} FROM {TableName} WHERE Email = @Email AND IsActive = 1";
        return await connection.QueryFirstOrDefaultAsync<Client>(sql, new { Email = email });
    }

    protected override string GetInsertSql()
    {
        return @"
            INSERT INTO Clients (FirstName, LastName, Email, Phone, DateOfBirth)
            OUTPUT INSERTED.Id
            VALUES (@FirstName, @LastName, @Email, @Phone, @DateOfBirth)";
    }

    protected override string GetUpdateSql()
    {
        return @"
            UPDATE Clients
            SET FirstName = @FirstName,
                LastName = @LastName,
                Email = @Email,
                Phone = @Phone,
                DateOfBirth = @DateOfBirth,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id AND IsActive = 1";
    }
}
