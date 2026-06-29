using Microsoft.AspNetCore.Mvc;
using OutEHR.Application.DTOs.Client;
using OutEHR.Application.DTOs.Common;
using OutEHR.Application.Interfaces;
using OutEHR.Domain.Entities;

namespace OutEHR.Api.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientRepository _repository;

    public ClientsController(IClientRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClientResponse>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Min(pageSize, 100);
        var result = await _repository.GetAllAsync(page, pageSize);
        return Ok(new PagedResult<ClientResponse>
        {
            Data = result.Data.Select(Map),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientResponse>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return NotFound();

        return Ok(Map(entity));
    }

    [HttpPost]
    public async Task<ActionResult<ClientResponse>> Create([FromBody] CreateClientRequest request)
    {
        var existing = await _repository.GetByEmailAsync(request.Email);
        if (existing is not null)
            return Conflict(new { message = $"Client with email '{request.Email}' already exists." });

        var entity = new Client
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, Map(created));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClientResponse>> Update(int id, [FromBody] UpdateClientRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        var duplicate = await _repository.GetByEmailAsync(request.Email);
        if (duplicate is not null && duplicate.Id != id)
            return Conflict(new { message = $"Client with email '{request.Email}' already exists." });

        existing.FirstName = request.FirstName;
        existing.LastName = request.LastName;
        existing.Email = request.Email;
        existing.Phone = request.Phone;
        existing.DateOfBirth = request.DateOfBirth;
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

    private static ClientResponse Map(Client entity) => new()
    {
        Id = entity.Id,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        Email = entity.Email,
        Phone = entity.Phone,
        DateOfBirth = entity.DateOfBirth,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
