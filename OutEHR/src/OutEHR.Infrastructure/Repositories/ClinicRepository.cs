using Dapper;
using OutEHR.Application.Interfaces;
using OutEHR.Domain.Entities;
using OutEHR.Infrastructure.Data;

namespace OutEHR.Infrastructure.Repositories;

public class ClinicRepository : BaseRepository<Clinic>, IClinicRepository
{
    protected override string TableName => "Clinics";
    protected override string SelectColumns => "Id, Name, Address, City, State, ZipCode, Phone, Email, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt";
    protected override string IdColumn => "Id";

    public ClinicRepository(DbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Clinic?> GetByNameAsync(string name)
    {
        using var connection = ConnectionFactory.CreateConnection();
        var sql = $"SELECT {SelectColumns} FROM {TableName} WHERE Name = @Name AND IsActive = 1";
        return await connection.QueryFirstOrDefaultAsync<Clinic>(sql, new { Name = name });
    }

    protected override string GetInsertSql()
    {
        return @"
            INSERT INTO Clinics (Name, Address, City, State, ZipCode, Phone, Email, Latitude, Longitude)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Address, @City, @State, @ZipCode, @Phone, @Email, @Latitude, @Longitude)";
    }

    protected override string GetUpdateSql()
    {
        return @"
            UPDATE Clinics
            SET Name = @Name,
                Address = @Address,
                City = @City,
                State = @State,
                ZipCode = @ZipCode,
                Phone = @Phone,
                Email = @Email,
                Latitude = @Latitude,
                Longitude = @Longitude,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id AND IsActive = 1";
    }
}
