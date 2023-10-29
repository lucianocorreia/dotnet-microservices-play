using System.Linq.Expressions;
using MongoDB.Driver;


namespace Play.Common.MongoDB;

public class MongoRepository<TEntity> : IRepository<TEntity> where TEntity : IEntity
{
    private readonly IMongoCollection<TEntity> dbCollection;
    private readonly FilterDefinitionBuilder<TEntity> filterBuilder = Builders<TEntity>.Filter;

    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        dbCollection = database.GetCollection<TEntity>(collectionName);
    }

    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync()
    {
        return await dbCollection.Find(filterBuilder.Empty).ToListAsync();
    }

    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await dbCollection.Find(filter).ToListAsync();
    }

    public async Task<TEntity> GetByIdAsync(Guid id)
    {
        var filter = filterBuilder.Eq(item => item.Id, id);
        return await dbCollection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<TEntity> GetByIdAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await dbCollection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(TEntity item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        await dbCollection.InsertOneAsync(item);
    }

    public async Task UpdateAsync(TEntity item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        var filter = filterBuilder.Eq(existingItem => existingItem.Id, item.Id);
        await dbCollection.ReplaceOneAsync(filter, item);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = filterBuilder.Eq(item => item.Id, id);
        await dbCollection.DeleteOneAsync(filter);
    }

}
