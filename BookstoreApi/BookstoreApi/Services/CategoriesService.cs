using BookstoreApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookstoreApi.Services;

public class CategoriesService
{
    private readonly IMongoCollection<Category> _categoryCollection;

    public CategoriesService(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            bookStoreDatabaseSettings.Value.DatabaseName);

        _categoryCollection = mongoDatabase.GetCollection<Category>(
            bookStoreDatabaseSettings.Value.CategoriesCollectionName);
    }

    public async Task<List<Category>> GetAsync()
    {
        return await _categoryCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Category?> GetCategoryNameAsync(string Category)
    {
        return await _categoryCollection.Find(x => x.CategoryName == Category).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Category newCategory)
    {
        await _categoryCollection.InsertOneAsync(newCategory);
    }

    public async Task UpdateAsync(string id, Category updatedCategory) =>
        await _categoryCollection.ReplaceOneAsync(x => x.Id == id, updatedCategory);

    public async Task RemoveAsync(string id) =>
        await _categoryCollection.DeleteOneAsync(x => x.Id == id);

}