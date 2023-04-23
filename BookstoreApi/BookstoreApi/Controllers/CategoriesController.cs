using BookstoreApi.Models;
using BookstoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoriesService _categoriesService;
    private readonly BooksService _booksService;
    public CategoriesController(CategoriesService categoriesService) =>
        _categoriesService = categoriesService;

    [HttpGet]
    public async Task<List<Category>> Get()
    {
        var allCategories = await _categoriesService.GetAsync();
        return allCategories;
    }

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Category>> Get(string id)
    {
        var category = await _categoriesService.GetAsync(id);
        if (category is null)
        {
            return NotFound();
        }
        return category;
    }

    [HttpGet("search")]
    public async Task<List<Category>> Search([FromQuery] string? name = null)
    {
        var allCategories = await _categoriesService.GetAsync();
        var filteredCategories = allCategories;

        if (!string.IsNullOrEmpty(name))
        {
            filteredCategories = filteredCategories.Where(b => b.CategoryName.ToLower().Contains(name.ToLower())).ToList();
        }
        return filteredCategories;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Category newCategory)
    {
        newCategory.Id = null;

        await _categoriesService.CreateAsync(newCategory);

        return CreatedAtAction(nameof(Get), new { id = newCategory.Id }, newCategory);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Category updatedCategory)
    {
        var category = await _categoriesService.GetAsync(id);
        
        if (category is null)
        {
            return NotFound();
        }
        


        updatedCategory.Id = category.Id;

        await _categoriesService.UpdateAsync(id, updatedCategory);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var category = await _categoriesService.GetAsync(id);
        
        if (category is null)
        {
            return NotFound();
        }
        await _categoriesService.RemoveAsync(id);

        return NoContent();
    }

}