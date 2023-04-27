using BookstoreApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookstoreApi.Services;

public class OrdersService
{
    private readonly IMongoCollection<Order> _ordersCollection;

    public OrdersService(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);
        
        var mongoDatabase = mongoClient.GetDatabase(
            bookStoreDatabaseSettings.Value.DatabaseName);

        _ordersCollection = mongoDatabase.GetCollection<Order>(
            bookStoreDatabaseSettings.Value.OrdersCollectionName);
    }

    public async Task<List<Order>> GetAsync()
    {
        return await _ordersCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Order?> GetAsync(string id)
    {
        return await _ordersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Order newOrder)
    {
        await _ordersCollection.InsertOneAsync(newOrder);         
    }

    public async Task UpdateAsync(string id, Order updatedOrder) =>
        await _ordersCollection.ReplaceOneAsync(x => x.Id == id, updatedOrder);

    public async Task RemoveAsync(string id) =>
        await _ordersCollection.DeleteOneAsync(x => x.Id == id);

}
