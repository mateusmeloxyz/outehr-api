using OutEHR.Domain.Entities;

namespace OutEHR.Application.Interfaces;

public interface IProviderRepository : IRepository<Provider>
{
    Task<Provider?> GetByEmailAsync(string email);
}
