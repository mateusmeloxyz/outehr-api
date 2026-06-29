using OutEHR.Application.DTOs.Common;

namespace OutEHR.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<PagedResult<T>> GetAllAsync(int page, int pageSize);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
