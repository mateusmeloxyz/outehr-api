using OutEHR.Domain.Entities;

namespace OutEHR.Application.Interfaces;

public interface IClinicRepository : IRepository<Clinic>
{
    Task<Clinic?> GetByNameAsync(string name);
}
