using BookstoreApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BookstoreApi.Services;

public class UsersService
{
    private readonly IMongoCollection<User> _userCollection;

    public UsersService(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            bookStoreDatabaseSettings.Value.DatabaseName);

        _userCollection = mongoDatabase.GetCollection<User>(
            bookStoreDatabaseSettings.Value.UsersCollectionName);
    }

    public async Task<List<User>> GetAsync() =>
        await _userCollection.Find(_ => true).ToListAsync();

    public async Task<User?> GetAsync(string Id) =>
        await _userCollection.Find(x => x.Id == Id).FirstOrDefaultAsync();

    public async Task CreateAsync(User newUser)
    {
        await _userCollection.InsertOneAsync(newUser);
    }

    public async Task UpdateAsync(string Id, User updatedUser) =>
        await _userCollection.ReplaceOneAsync(x => x.Id == Id, updatedUser);

    public async Task RemoveAsync(string userName) =>
        await _userCollection.DeleteOneAsync(x => x.UserName == userName);
}