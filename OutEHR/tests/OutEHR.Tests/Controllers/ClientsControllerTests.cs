using Microsoft.AspNetCore.Mvc;
using Moq;
using OutEHR.Api.Controllers;
using OutEHR.Application.Interfaces;
using OutEHR.Domain.Entities;
using Xunit;

namespace OutEHR.Tests.Controllers;

public class ClientsControllerTests
{
    private readonly Mock<IClientRepository> _repoMock;
    private readonly ClientsController _controller;

    public ClientsControllerTests()
    {
        _repoMock = new Mock<IClientRepository>();
        _controller = new ClientsController(_repoMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithPagedResult()
    {
        _repoMock.Setup(r => r.GetAllAsync(1, 10)).ReturnsAsync(new Application.DTOs.Common.PagedResult<Client>
        {
            Data = [],
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        });

        var result = await _controller.GetAll();

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        var client = new Client { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(client);

        var result = await _controller.GetById(1);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Client?)null);

        var result = await _controller.GetById(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_Valid_Returns201()
    {
        var request = new Application.DTOs.Client.CreateClientRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@test.com"
        };
        var created = new Client { Id = 1, FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", IsActive = true };

        _repoMock.Setup(r => r.GetByEmailAsync("jane@test.com")).ReturnsAsync((Client?)null);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Client>())).ReturnsAsync(created);

        var result = await _controller.Create(request);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Create_DuplicateEmail_Returns409()
    {
        var request = new Application.DTOs.Client.CreateClientRequest { FirstName = "Jane", LastName = "Doe", Email = "jane@test.com" };
        _repoMock.Setup(r => r.GetByEmailAsync("jane@test.com")).ReturnsAsync(new Client { Id = 1 });

        var result = await _controller.Create(request);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        _repoMock.Setup(r => r.SoftDeleteAsync(1)).ReturnsAsync(true);

        var result = await _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        _repoMock.Setup(r => r.SoftDeleteAsync(99)).ReturnsAsync(false);

        var result = await _controller.Delete(99);

        Assert.IsType<NotFoundResult>(result);
    }
}
