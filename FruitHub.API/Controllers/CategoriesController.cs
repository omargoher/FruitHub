using System.Collections.Immutable;
using FruitHub.ApplicationCore.DTOs.Category;
using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly  ICategoryService _categoryService;
    private readonly  IProductService _productService;

    public CategoriesController(ICategoryService categoryService, IProductService productService)
    {
        _categoryService = categoryService;
        _productService = productService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var categories = await _categoryService
            .GetAllAsync();

        return Ok(categories);
    }
    
    [HttpGet("{categoryId:int}")]
    public async Task<IActionResult> GetByIdAsync(int categoryId)
    {
        var category = await _categoryService
            .GetByIdAsync(categoryId);

        return Ok(category);
    }
    
    [HttpGet("{categoryId:int}/products")]
    public async Task<IActionResult> GetProductsAsync(int categoryId, [FromQuery]ProductQuery productQuery)
    {
        var products = await _productService
            .GetByCategoryAsync(categoryId, productQuery);
       
        return Ok(products);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody]CategoryDto dto)
    {
        var category = await _categoryService.CreateAsync(dto);

        return CreatedAtAction(
            "GetById",
            new { categoryId = category.Id },
            category);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("{categoryId:int}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute]int categoryId, 
        [FromBody]CategoryDto dto)
    {
        await _categoryService.UpdateAsync(categoryId, dto);
        
        return NoContent();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{categoryId:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int categoryId)
    {
        await _categoryService.DeleteAsync(categoryId);
        
        return NoContent();
    }
}