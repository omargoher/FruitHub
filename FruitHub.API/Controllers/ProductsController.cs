using System.Security.Claims;
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
    
    /*
     * TODO
     * http://localhost:5259/api/products?sortby=mostselling => try this when add orders to check it
     *
     * TODO
     * http://localhost:5259/api/products?sortby=price&sortdir=desc => error with sqlite => try it with sqlserver
     */
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] ProductQuery query)
    {
        var products = await _productService.GetAllAsync(query);

        if (!products.Any())
        {
            return NoContent();
        }
        
        return Ok(products);
    }

    [HttpGet("{productId:int}")]
    public async Task<IActionResult> GetByIdAsync(int productId)
    {
        var product = await _productService.GetByIdAsync(productId);
        
        if (product == null)
        {
            return NoContent();
        }
        
        return Ok(product);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateAsync([FromForm]CreateProductDto dto, [FromForm]IFormFile image)
    {
        var adminId = User.FindFirstValue("business_admin_id");
        if (adminId == null)
        {
            return Unauthorized();
        }
        
        var imageDto = new ImageDto
        {
            Content = image.OpenReadStream(),
            FileName = image.FileName,
            ContentType = image.ContentType
        };
        
        await _productService.CreateAsync(int.Parse(adminId), dto, imageDto);
        
        return Created();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPatch("{productId:int}")]
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
                FileName = image.FileName,
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