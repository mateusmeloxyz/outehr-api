using OutEHR.Application.Interfaces;
using OutEHR.Domain.Entities;
using OutEHR.Infrastructure.Data;
using OutEHR.Infrastructure.Repositories;
using Xunit;

namespace OutEHR.Tests.Repositories;

public class SpecialtyRepositoryTests : IAsyncLifetime
{
    private readonly DbConnectionFactory _factory;
    private readonly ISpecialtyRepository _repository;
    private readonly List<int> _createdIds = [];

    public SpecialtyRepositoryTests()
    {
        var connectionString = "Server=localhost,1433;Database=OutEHR;User Id=sa;Password=418f832%jsalov89!;TrustServerCertificate=True";
        _factory = new DbConnectionFactory(connectionString);
        _repository = new SpecialtyRepository(_factory);
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var id in _createdIds)
        {
            try { await _repository.SoftDeleteAsync(id); } catch { }
        }
    }

    [Fact]
    public async Task GetAll_ReturnsPagedResult()
    {
        var result = await _repository.GetAllAsync(1, 5);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 6);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsEntity()
    {
        var entity = await _repository.GetByIdAsync(1);
        Assert.NotNull(entity);
        Assert.Equal("Orthopedics", entity.Name);
    }

    [Fact]
    public async Task GetById_NonExisting_ReturnsNull()
    {
        var entity = await _repository.GetByIdAsync(9999);
        Assert.Null(entity);
    }

    [Fact]
    public async Task AddAsync_CreatesAndReturnsEntity()
    {
        var entity = new Specialty
        {
            Name = "Test Specialty " + Guid.NewGuid().ToString("N")[..8],
            Description = "Integration test",
            DefaultSlotDurationMinutes = 20
        };

        var created = await _repository.AddAsync(entity);
        _createdIds.Add(created.Id);

        Assert.NotEqual(0, created.Id);
        Assert.Equal(entity.Name, created.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEntity()
    {
        var entity = new Specialty
        {
            Name = "Update Test " + Guid.NewGuid().ToString("N")[..8],
            DefaultSlotDurationMinutes = 15
        };
        var created = await _repository.AddAsync(entity);
        _createdIds.Add(created.Id);

        created.DefaultSlotDurationMinutes = 45;
        await _repository.UpdateAsync(created);

        var updated = await _repository.GetByIdAsync(created.Id);
        Assert.NotNull(updated);
        Assert.Equal(45, updated.DefaultSlotDurationMinutes);
    }

    [Fact]
    public async Task SoftDeleteAsync_SetsInactive()
    {
        var entity = new Specialty
        {
            Name = "Delete Test " + Guid.NewGuid().ToString("N")[..8],
            DefaultSlotDurationMinutes = 10
        };
        var created = await _repository.AddAsync(entity);
        _createdIds.Add(created.Id);

        var deleted = await _repository.SoftDeleteAsync(created.Id);
        Assert.True(deleted);

        var fetched = await _repository.GetByIdAsync(created.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetByNameAsync_FindsByName()
    {
        var entity = new Specialty
        {
            Name = "NameLookup " + Guid.NewGuid().ToString("N")[..8],
            DefaultSlotDurationMinutes = 10
        };
        var created = await _repository.AddAsync(entity);
        _createdIds.Add(created.Id);

        var found = await _repository.GetByNameAsync(entity.Name);
        Assert.NotNull(found);
        Assert.Equal(created.Id, found.Id);
    }
}
