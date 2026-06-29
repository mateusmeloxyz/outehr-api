using Microsoft.AspNetCore.Mvc;
using OutEHR.Application.DTOs.Clinic;
using OutEHR.Application.DTOs.Common;
using OutEHR.Application.Interfaces;
using OutEHR.Domain.Entities;

namespace OutEHR.Api.Controllers;

[ApiController]
[Route("api/clinics")]
public class ClinicsController : ControllerBase
{
    private readonly IClinicRepository _repository;

    public ClinicsController(IClinicRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClinicResponse>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Min(pageSize, 100);
        var result = await _repository.GetAllAsync(page, pageSize);
        return Ok(new PagedResult<ClinicResponse>
        {
            Data = result.Data.Select(Map),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClinicResponse>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return NotFound();

        return Ok(Map(entity));
    }

    [HttpPost]
    public async Task<ActionResult<ClinicResponse>> Create([FromBody] CreateClinicRequest request)
    {
        var existing = await _repository.GetByNameAsync(request.Name);
        if (existing is not null)
            return Conflict(new { message = $"Clinic '{request.Name}' already exists." });

        var entity = new Clinic
        {
            Name = request.Name,
            Address = request.Address,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            Phone = request.Phone,
            Email = request.Email,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, Map(created));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClinicResponse>> Update(int id, [FromBody] UpdateClinicRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        var duplicate = await _repository.GetByNameAsync(request.Name);
        if (duplicate is not null && duplicate.Id != id)
            return Conflict(new { message = $"Clinic '{request.Name}' already exists." });

        existing.Name = request.Name;
        existing.Address = request.Address;
        existing.City = request.City;
        existing.State = request.State;
        existing.ZipCode = request.ZipCode;
        existing.Phone = request.Phone;
        existing.Email = request.Email;
        existing.Latitude = request.Latitude;
        existing.Longitude = request.Longitude;
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

    private static ClinicResponse Map(Clinic entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Address = entity.Address,
        City = entity.City,
        State = entity.State,
        ZipCode = entity.ZipCode,
        Phone = entity.Phone,
        Email = entity.Email,
        Latitude = entity.Latitude,
        Longitude = entity.Longitude,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
