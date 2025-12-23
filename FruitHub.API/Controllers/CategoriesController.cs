using FruitHub.ApplicationCore.DTOs.Category;
using FruitHub.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly  ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var categories = await _categoryService.GetAllAsync();
        
        return Ok(categories);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody]CreateCategoryDto dto)
    {
        await _categoryService.CreateAsync(dto.Name);
        
        return Created();
    }
    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute]int id, [FromBody]UpdateCategoryDto dto)
    {
        dto.Id = id;
        await _categoryService.UpdateAsync(dto);
        
        return NoContent();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
        await _categoryService.DeleteAsync(id);
        
        return NoContent();
    }
}