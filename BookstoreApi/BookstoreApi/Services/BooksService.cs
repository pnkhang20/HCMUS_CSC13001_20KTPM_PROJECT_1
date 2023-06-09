﻿using BookstoreApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BookstoreApi.Services;

public class BooksService
{
    private readonly IMongoCollection<Book> _booksCollection;

    public BooksService(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            bookStoreDatabaseSettings.Value.DatabaseName);

        _booksCollection = mongoDatabase.GetCollection<Book>(
            bookStoreDatabaseSettings.Value.BooksCollectionName);
    }

    public async Task<List<Book>> GetAsync()
    {
        return await _booksCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Book?> GetAsync(string id)
    {
        return await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Book newBook)
    {
        await _booksCollection.InsertOneAsync(newBook);
    }

    public async Task UpdateAsync(string id, Book updatedBook) =>
        await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(string id) =>
        await _booksCollection.DeleteOneAsync(x => x.Id == id);

    public async Task<Book?> GetBookCategoryAsync(string category)
    {
        return await _booksCollection.Find(x => x.Category.Id == category).FirstOrDefaultAsync();
    }
    public async Task UpdateDeletedCategoryAsync(string categoryId)
    {
        var updatedCategory = new Category { Id = null, CategoryName = "" };
        var filter = Builders<Book>.Filter.Eq(x => x.Category.Id, categoryId);
        var update = Builders<Book>.Update.Set(x => x.Category, updatedCategory);
        await _booksCollection.UpdateManyAsync(filter, update);
    }

    public async Task UpdateCategoryNameAsync(string categoryId, string updatedCategoryName)
    {
        var filter = Builders<Book>.Filter.Eq(x => x.Category.Id, categoryId);
        var update = Builders<Book>.Update.Set(x => x.Category.CategoryName, updatedCategoryName);
        await _booksCollection.UpdateManyAsync(filter, update);
    }


    public async Task UpdateManyAsync(FilterDefinition<Book> filter, UpdateDefinition<Book> update)
    {
        await _booksCollection.UpdateManyAsync(filter, update);
    }

}