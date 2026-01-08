using System.Security.Claims;
using FruitHub.API.Extensions;
using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> GetAllAsync([FromQuery] ProductQuery query)
    {
        var products = await _productService.GetAllAsync(query);

        return Ok(products);
    }

    [HttpGet("{productId:int}")]
    public async Task<IActionResult> GetByIdAsync(int productId)
    {
        var product = await _productService.GetByIdAsync(productId);
        
        return Ok(product);
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateAsync([FromForm]CreateProductDto dto, [FromForm]IFormFile image)
    {
        var adminId = ClaimsPrincipalExtensions.GetAdminId(User);
        
        var imageDto = new ImageDto
        {
            Content = image.OpenReadStream(),
            Length = image.Length,
            ContentType = image.ContentType
        };
        
        
        await _productService.CreateAsync(adminId, dto, imageDto);
        
        return Created();
    }
    
    [HttpPatch("{productId:int}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute]int productId, 
        [FromForm]UpdateProductDto dto, 
        [FromForm]IFormFile? image = null)
    {
        ImageDto? imageDto = null;
        
        if (image != null)
        {
            imageDto = new ImageDto
            {
                Content = image.OpenReadStream(),
                Length = image.Length,
                ContentType = image.ContentType
            };
        }

        await _productService.UpdateAsync(productId, dto, imageDto);
            
        return NoContent();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> DeleteAsync(int productId)
    {
        await _productService.DeleteAsync(productId);
        
        return NoContent();
    }
}