using Microsoft.AspNetCore.Mvc;
using OutEHR.Application.DTOs.Common;
using OutEHR.Application.DTOs.Provider;
using OutEHR.Application.Interfaces;
using OutEHR.Domain.Entities;

namespace OutEHR.Api.Controllers;

[ApiController]
[Route("api/providers")]
public class ProvidersController : ControllerBase
{
    private readonly IProviderRepository _repository;
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly IClinicRepository _clinicRepository;

    public ProvidersController(
        IProviderRepository repository,
        ISpecialtyRepository specialtyRepository,
        IClinicRepository clinicRepository)
    {
        _repository = repository;
        _specialtyRepository = specialtyRepository;
        _clinicRepository = clinicRepository;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProviderResponse>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Min(pageSize, 100);
        var result = await _repository.GetAllAsync(page, pageSize);
        return Ok(new PagedResult<ProviderResponse>
        {
            Data = result.Data.Select(Map),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProviderResponse>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return NotFound();

        return Ok(Map(entity));
    }

    [HttpPost]
    public async Task<ActionResult<ProviderResponse>> Create([FromBody] CreateProviderRequest request)
    {
        if (!string.IsNullOrEmpty(request.Email))
        {
            var emailExists = await _repository.GetByEmailAsync(request.Email);
            if (emailExists is not null)
                return Conflict(new { message = $"Provider with email '{request.Email}' already exists." });
        }

        if (!await _specialtyRepository.ExistsAsync(request.SpecialtyId))
            return BadRequest(new { message = $"Specialty with Id {request.SpecialtyId} not found." });

        if (!await _clinicRepository.ExistsAsync(request.ClinicId))
            return BadRequest(new { message = $"Clinic with Id {request.ClinicId} not found." });

        var entity = new Provider
        {
            SpecialtyId = request.SpecialtyId,
            ClinicId = request.ClinicId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            NPI = request.NPI,
            Phone = request.Phone,
            Email = request.Email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, Map(created));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProviderResponse>> Update(int id, [FromBody] UpdateProviderRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        if (!string.IsNullOrEmpty(request.Email))
        {
            var emailExists = await _repository.GetByEmailAsync(request.Email);
            if (emailExists is not null && emailExists.Id != id)
                return Conflict(new { message = $"Provider with email '{request.Email}' already exists." });
        }

        if (!await _specialtyRepository.ExistsAsync(request.SpecialtyId))
            return BadRequest(new { message = $"Specialty with Id {request.SpecialtyId} not found." });

        if (!await _clinicRepository.ExistsAsync(request.ClinicId))
            return BadRequest(new { message = $"Clinic with Id {request.ClinicId} not found." });

        existing.SpecialtyId = request.SpecialtyId;
        existing.ClinicId = request.ClinicId;
        existing.FirstName = request.FirstName;
        existing.LastName = request.LastName;
        existing.NPI = request.NPI;
        existing.Phone = request.Phone;
        existing.Email = request.Email;
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

    private static ProviderResponse Map(Provider entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        SpecialtyId = entity.SpecialtyId,
        ClinicId = entity.ClinicId,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        NPI = entity.NPI,
        Phone = entity.Phone,
        Email = entity.Email,
        Rating = entity.Rating,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
