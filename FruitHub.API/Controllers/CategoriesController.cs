using FruitHub.ApplicationCore.DTOs.Category;
using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Interfaces.Services;
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
        var categories = await _categoryService.GetAllAsync();

        if (!categories.Any())
        {
            return NoContent();
        }
        
        return Ok(categories);
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductsAsync(int id, [FromQuery]ProductQuery productQuery)
    {
        var products = await _productService.GetByCategoryAsync(id, productQuery);
        
        if (!products.Any())
        {
            return NoContent();
        }
        
        return Ok(products);
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