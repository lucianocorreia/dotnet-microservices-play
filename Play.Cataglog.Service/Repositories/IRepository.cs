using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Repositories;

public interface IRepository<TEntity>
{
    Task<IReadOnlyCollection<TEntity>> GetAllAsync();
    Task<TEntity> GetByIdAsync(Guid id);
    Task CreateAsync(TEntity item);
    Task UpdateAsync(TEntity item);
    Task DeleteAsync(Guid id);
}
