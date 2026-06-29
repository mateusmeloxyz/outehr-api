using OutEHR.Domain.Entities;

namespace OutEHR.Application.Interfaces;

public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetByEmailAsync(string email);
}
