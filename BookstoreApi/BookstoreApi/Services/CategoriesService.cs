﻿using BookstoreApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BookstoreApi.Services;

public class CategoriesService
{
    private readonly IMongoCollection<Category> _categoryCollection;
    private readonly BooksService _booksService;

    public CategoriesService(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings,
        BooksService booksService)
    {
        var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            bookStoreDatabaseSettings.Value.DatabaseName);

        _categoryCollection = mongoDatabase.GetCollection<Category>(
            bookStoreDatabaseSettings.Value.CategoriesCollectionName);
        _booksService = booksService;
    }

    public async Task<List<Category>> GetAsync()
    {
        return await _categoryCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Category?> GetAsync(string Id)
    {
        return await _categoryCollection.Find(x => x.Id == Id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Category newCategory)
    {
        await _categoryCollection.InsertOneAsync(newCategory);
    }

    public async Task UpdateAsync(string id, Category updatedCategory)
    {

        await _categoryCollection.ReplaceOneAsync(x => x.Id == id, updatedCategory);
        await _booksService.UpdateCategoryNameAsync(id, updatedCategory.CategoryName);
    }


    public async Task RemoveAsync(string id)
    {
        var category = await _categoryCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (category != null)
        {
            // Update the CategoryId to empty string in all books with the deleted category id
            await _booksService.UpdateDeletedCategoryAsync(id);

            // Delete the category
            await _categoryCollection.DeleteOneAsync(x => x.Id == id);
        }

    }

}