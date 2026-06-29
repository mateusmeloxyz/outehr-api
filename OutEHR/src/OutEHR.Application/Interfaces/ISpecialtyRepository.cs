using OutEHR.Domain.Entities;

namespace OutEHR.Application.Interfaces;

public interface ISpecialtyRepository : IRepository<Specialty>
{
    Task<Specialty?> GetByNameAsync(string name);
}
