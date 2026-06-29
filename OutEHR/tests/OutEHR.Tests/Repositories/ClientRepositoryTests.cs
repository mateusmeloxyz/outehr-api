using Dapper;
using OutEHR.Application.Interfaces;
using OutEHR.Domain.Entities;
using OutEHR.Infrastructure.Data;
using OutEHR.Infrastructure.Repositories;
using Xunit;

namespace OutEHR.Tests.Repositories;

public class ClientRepositoryTests : IAsyncLifetime
{
    private readonly DbConnectionFactory _factory;
    private readonly IClientRepository _repository;
    private readonly List<int> _createdIds = [];

    static ClientRepositoryTests()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }

    public ClientRepositoryTests()
    {
        var connectionString = "Server=localhost,1433;Database=OutEHR;User Id=sa;Password=418f832%jsalov89!;TrustServerCertificate=True";
        _factory = new DbConnectionFactory(connectionString);
        _repository = new ClientRepository(_factory);
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
        var result = await _repository.GetAllAsync(1, 10);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AddAsync_CreatesClient()
    {
        var client = new Client
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"test-{Guid.NewGuid():N}@outehr-test.com",
            Phone = "555-0000"
        };

        var created = await _repository.AddAsync(client);
        _createdIds.Add(created.Id);

        Assert.NotEqual(0, created.Id);
        Assert.Equal(client.Email, created.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_FindsByEmail()
    {
        var email = $"lookup-{Guid.NewGuid():N}@outehr-test.com";
        var client = new Client
        {
            FirstName = "Lookup",
            LastName = "Test",
            Email = email
        };
        var created = await _repository.AddAsync(client);
        _createdIds.Add(created.Id);

        var found = await _repository.GetByEmailAsync(email);
        Assert.NotNull(found);
        Assert.Equal(created.Id, found.Id);
    }

    [Fact]
    public async Task SoftDeleteAsync_MakesInactive()
    {
        var client = new Client
        {
            FirstName = "Delete",
            LastName = "Me",
            Email = $"delete-{Guid.NewGuid():N}@outehr-test.com"
        };
        var created = await _repository.AddAsync(client);
        _createdIds.Add(created.Id);

        var deleted = await _repository.SoftDeleteAsync(created.Id);
        Assert.True(deleted);

        var fetched = await _repository.GetByIdAsync(created.Id);
        Assert.Null(fetched);
    }
}
