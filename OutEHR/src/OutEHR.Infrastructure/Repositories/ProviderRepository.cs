using Dapper;
using OutEHR.Application.Interfaces;
using OutEHR.Domain.Entities;
using OutEHR.Infrastructure.Data;

namespace OutEHR.Infrastructure.Repositories;

public class ProviderRepository : BaseRepository<Provider>, IProviderRepository
{
    protected override string TableName => "Providers";
    protected override string SelectColumns => "Id, UserId, SpecialtyId, ClinicId, FirstName, LastName, NPI, Phone, Email, Rating, IsActive, CreatedAt, UpdatedAt";
    protected override string IdColumn => "Id";

    public ProviderRepository(DbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Provider?> GetByEmailAsync(string email)
    {
        using var connection = ConnectionFactory.CreateConnection();
        var sql = $"SELECT {SelectColumns} FROM {TableName} WHERE Email = @Email AND IsActive = 1";
        return await connection.QueryFirstOrDefaultAsync<Provider>(sql, new { Email = email });
    }

    protected override string GetInsertSql()
    {
        return @"
            INSERT INTO Providers (SpecialtyId, ClinicId, FirstName, LastName, NPI, Phone, Email)
            OUTPUT INSERTED.Id
            VALUES (@SpecialtyId, @ClinicId, @FirstName, @LastName, @NPI, @Phone, @Email)";
    }

    protected override string GetUpdateSql()
    {
        return @"
            UPDATE Providers
            SET SpecialtyId = @SpecialtyId,
                ClinicId = @ClinicId,
                FirstName = @FirstName,
                LastName = @LastName,
                NPI = @NPI,
                Phone = @Phone,
                Email = @Email,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id AND IsActive = 1";
    }
}
