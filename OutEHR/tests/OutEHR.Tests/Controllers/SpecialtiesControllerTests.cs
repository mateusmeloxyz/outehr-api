using Microsoft.AspNetCore.Mvc;
using Moq;
using OutEHR.Api.Controllers;
using OutEHR.Application.Interfaces;
using OutEHR.Domain.Entities;
using Xunit;

namespace OutEHR.Tests.Controllers;

public class SpecialtiesControllerTests
{
    private readonly Mock<ISpecialtyRepository> _repoMock;
    private readonly SpecialtiesController _controller;

    public SpecialtiesControllerTests()
    {
        _repoMock = new Mock<ISpecialtyRepository>();
        _controller = new SpecialtiesController(_repoMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithPagedResult()
    {
        _repoMock.Setup(r => r.GetAllAsync(1, 10)).ReturnsAsync(new Application.DTOs.Common.PagedResult<Specialty>
        {
            Data = [],
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        });

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        var specialty = new Specialty { Id = 1, Name = "Orthopedics", DefaultSlotDurationMinutes = 15 };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(specialty);

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Specialty?)null);

        var result = await _controller.GetById(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_Valid_Returns201()
    {
        var request = new Application.DTOs.Specialty.CreateSpecialtyRequest { Name = "Cardiology", DefaultSlotDurationMinutes = 30 };
        var created = new Specialty { Id = 10, Name = "Cardiology", DefaultSlotDurationMinutes = 30, IsActive = true };

        _repoMock.Setup(r => r.GetByNameAsync("Cardiology")).ReturnsAsync((Specialty?)null);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Specialty>())).ReturnsAsync(created);

        var result = await _controller.Create(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(SpecialtiesController.GetById), createdResult.ActionName);
    }

    [Fact]
    public async Task Create_DuplicateName_Returns409()
    {
        var request = new Application.DTOs.Specialty.CreateSpecialtyRequest { Name = "Orthopedics", DefaultSlotDurationMinutes = 30 };
        _repoMock.Setup(r => r.GetByNameAsync("Orthopedics")).ReturnsAsync(new Specialty { Id = 1 });

        var result = await _controller.Create(request);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_Existing_ReturnsOk()
    {
        var existing = new Specialty { Id = 1, Name = "Orthopedics", DefaultSlotDurationMinutes = 15, IsActive = true };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.GetByNameAsync("Orthopedics")).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Specialty>())).ReturnsAsync(existing);

        var request = new Application.DTOs.Specialty.UpdateSpecialtyRequest { Name = "Orthopedics", DefaultSlotDurationMinutes = 20 };
        var result = await _controller.Update(1, request);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Specialty?)null);

        var request = new Application.DTOs.Specialty.UpdateSpecialtyRequest { Name = "X", DefaultSlotDurationMinutes = 10 };
        var result = await _controller.Update(99, request);

        Assert.IsType<NotFoundResult>(result.Result);
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
