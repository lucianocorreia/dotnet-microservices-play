using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Repositories;

public interface IItemsRepository
{
    Task<IReadOnlyCollection<Item>> GetAllAsync();
    Task<Item> GetByIdAsync(Guid id);
    Task CreateAsync(Item item);
    Task UpdateAsync(Item item);
    Task DeleteAsync(Guid id);
}
