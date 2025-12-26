using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly  IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] ProductQuery query)
    {
        var products = await _productService.GetAllAsync(query);

        if (!products.Any())
        {
            return NoContent();
        }
        
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        
        if (product == null)
        {
            return NoContent();
        }
        
        return Ok(product);
    }
    
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateAsync([FromForm]CreateProductDto dto, [FromForm]IFormFile image)
    {
        var imageDto = new ImageDto
        {
            Content = image.OpenReadStream(),
            FileName = image.FileName,
            ContentType = image.ContentType
        };
        
        await _productService.CreateAsync(dto, imageDto);
        
        return Created();
    }
    
    [HttpPatch("{id:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute]int id, 
        [FromForm]UpdateProductDto dto, 
        [FromForm]IFormFile? image = null)
    {
        dto.Id = id;
        ImageDto? imageDto = null;
        
        if (image != null)
        {
            imageDto = new ImageDto
            {
                Content = image.OpenReadStream(),
                FileName = image.FileName,
                ContentType = image.ContentType
            };
        }

        await _productService.UpdateAsync(dto, imageDto);
            
        return NoContent();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
        await _productService.DeleteAsync(id);
        
        return NoContent();
    }
}