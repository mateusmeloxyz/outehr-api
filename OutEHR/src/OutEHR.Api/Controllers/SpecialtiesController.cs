using Microsoft.AspNetCore.Mvc;
using OutEHR.Application.DTOs.Common;
using OutEHR.Application.DTOs.Specialty;
using OutEHR.Application.Interfaces;
using OutEHR.Domain.Entities;

namespace OutEHR.Api.Controllers;

[ApiController]
[Route("api/specialties")]
public class SpecialtiesController : ControllerBase
{
    private readonly ISpecialtyRepository _repository;

    public SpecialtiesController(ISpecialtyRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<SpecialtyResponse>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Min(pageSize, 100);
        var result = await _repository.GetAllAsync(page, pageSize);
        return Ok(new PagedResult<SpecialtyResponse>
        {
            Data = result.Data.Select(Map),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SpecialtyResponse>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return NotFound();

        return Ok(Map(entity));
    }

    [HttpPost]
    public async Task<ActionResult<SpecialtyResponse>> Create([FromBody] CreateSpecialtyRequest request)
    {
        var existing = await _repository.GetByNameAsync(request.Name);
        if (existing is not null)
            return Conflict(new { message = $"Specialty '{request.Name}' already exists." });

        var entity = new Specialty
        {
            Name = request.Name,
            Description = request.Description,
            DefaultSlotDurationMinutes = request.DefaultSlotDurationMinutes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, Map(created));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SpecialtyResponse>> Update(int id, [FromBody] UpdateSpecialtyRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        var duplicate = await _repository.GetByNameAsync(request.Name);
        if (duplicate is not null && duplicate.Id != id)
            return Conflict(new { message = $"Specialty '{request.Name}' already exists." });

        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.DefaultSlotDurationMinutes = request.DefaultSlotDurationMinutes;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        return Ok(Map(updated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _repository.SoftDeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private static SpecialtyResponse Map(Specialty entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        DefaultSlotDurationMinutes = entity.DefaultSlotDurationMinutes,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
