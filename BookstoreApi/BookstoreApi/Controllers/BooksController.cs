using BookstoreApi.Models;
using BookstoreApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookstoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BooksService _booksService;
        private readonly CategoriesService _categoriesService;

        public BooksController(BooksService booksService, CategoriesService categoriesService)
        {
            _booksService = booksService;
            _categoriesService = categoriesService;
        }

        [HttpGet]
        public async Task<List<Book>> Get()
        {
            var allBooks = await _booksService.GetAsync();
            return allBooks;
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Book>> Get(string id)
        {
            var book = await _booksService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            return book;
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
                filteredBooks = filteredBooks.Where(b => b.Title.ToLower().Contains(name.ToLower())).ToList();
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
            try
            {
                // Look up the Category by ID in the database            
                var category = await _categoriesService.GetAsync(newBook.Category.Id);

                if (category == null)
                {
                    return BadRequest($"Category with ID {newBook.Category.Id} not found.");
                }
                newBook.Id = null;
                newBook.Category.Id = category.Id;
                newBook.Category.CategoryName = category.CategoryName;
                // Add the new Book to the database
                await _booksService.CreateAsync(newBook);

                return Ok(newBook);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Book updatedBook)
        {
            var book = await _booksService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            updatedBook.Id = book.Id;

            // Look up the Category by ID in the database
            var category = await _categoriesService.GetAsync(updatedBook.Category.Id);

            if (category == null)
            {
                return BadRequest($"Category with ID {updatedBook.Category.Id} not found.");
            }

            // Associate the Category with the Book
            updatedBook.Category.Id = category.Id;
            updatedBook.Category.CategoryName = category.CategoryName;
            await _booksService.UpdateAsync(id, updatedBook);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var book = await _booksService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            await _booksService.RemoveAsync(id);

            return NoContent();
        }
    }
}