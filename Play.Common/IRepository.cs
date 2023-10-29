using System.Linq.Expressions;

namespace Play.Common;

public interface IRepository<TEntity>
{
    Task<IReadOnlyCollection<TEntity>> GetAllAsync();
    Task<IReadOnlyCollection<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter);
    Task<TEntity> GetByIdAsync(Guid id);
    Task<TEntity> GetByIdAsync(Expression<Func<TEntity, bool>> filter);
    Task CreateAsync(TEntity item);
    Task UpdateAsync(TEntity item);
    Task DeleteAsync(Guid id);
}
