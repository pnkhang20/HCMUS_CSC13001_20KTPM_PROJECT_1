using BookstoreApi.Models;
using BookstoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BooksService _booksService;

    public BooksController(BooksService booksService) =>
        _booksService = booksService;

    [HttpGet]
    public async Task<List<Book>> Get()
    {
        var allBooks = await _booksService.GetAsync();
        return allBooks;
    }
    
    [HttpGet("search")]
    public async Task<List<Book>> Search(
    [FromQuery] string? name = null,
    [FromQuery] decimal? minPrice = null,
    [FromQuery] decimal? maxPrice = null)
    {
        var allBooks = await _booksService.GetAsync();
        var filteredBooks = allBooks;

        if (!string.IsNullOrEmpty(name))
        {
            filteredBooks = filteredBooks.Where(b => b.BookName.ToLower().Contains(name.ToLower())).ToList();
        }

        if (minPrice.HasValue)
        {
            filteredBooks = filteredBooks.Where(b => b.Price >= minPrice.Value).ToList();
        }

        if (maxPrice.HasValue)
        {
            filteredBooks = filteredBooks.Where(b => b.Price <= maxPrice.Value).ToList();
        }

        return filteredBooks;
    }


    [HttpPost]
    public async Task<IActionResult> Post(Book newBook)
    {
        await _booksService.CreateAsync(newBook);

        return CreatedAtAction(nameof(Get), new { id = newBook.Id }, newBook);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Book updatedBook)
    {
        var book = await _booksService.GetBookNameAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        updatedBook.Id = book.Id;

        await _booksService.UpdateAsync(id, updatedBook);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var book = await _booksService.GetBookNameAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        await _booksService.RemoveAsync(id);

        return NoContent();
    }

}