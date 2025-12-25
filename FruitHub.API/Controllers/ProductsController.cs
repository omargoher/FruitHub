using FruitHub.ApplicationCore.DTOs.Category;
using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly  IProductService _productService;
    private readonly  IImageService _imageService;

    public ProductsController(IProductService productService,  IImageService imageService)
    {
        _productService = productService;
        _imageService = imageService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] string? search)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var result = await _productService.SearchAsync(search);
            return Ok(result);
        }

        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var categories = await _productService.GetByIdAsync(id);
        
        return Ok(categories);
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
    public async Task<IActionResult> UpdateAsync([FromRoute]int id, [FromForm]UpdateProductDto dto, [FromForm]IFormFile? image = null)
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