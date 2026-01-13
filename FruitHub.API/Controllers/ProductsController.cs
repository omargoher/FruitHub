using System.Security.Claims;
using FruitHub.API.Extensions;
using FruitHub.API.Requests;
using FruitHub.API.Responses;
using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

/// <summary>
/// Manage products in the catalog
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Products")]
public class ProductsController : ControllerBase
{
    private readonly  IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }
    
    /// <summary>
    /// Get all products with optional filtering, sorting, and pagination
    /// </summary>
    /// <param name="query">Product query parameters</param>
    /// <returns>List of products</returns>
    /// <response code="200">Products retrieved successfully</response>
    /// <response code="400">Invalid query data</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllAsync([FromQuery] ProductQuery query)
    {
        var products = await _productService.GetAllAsync(query);
        return Ok(products);
    }

    /// <summary>
    /// Get a single product by its identifier
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <returns>Product details</returns>
    /// <response code="200">Product retrieved successfully</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{productId:int}")]
    [ProducesResponseType(typeof(SingleProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int productId)
    {
        var product = await _productService.GetByIdAsync(productId);
        return Ok(product);
    }
    
    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="request">Product creation data</param>
    /// <returns>The created product</returns>
    /// <remarks>
    /// Requires admin authorization.  
    /// Content-Type must be <c>multipart/form-data</c>.
    /// </remarks>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid product data or image</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User is not authorized (Admin only)</response>
    /// <response code="404">category not found</response>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(SingleProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAsync(
        [FromForm] CreateProductRequest request)
    {
        var adminId = ClaimsPrincipalExtensions.GetAdminId(User);

        var dto = new CreateProductDto
        {
            Name = request.Name,
            Price = request.Price,
            Calories = request.Calories,
            Description = request.Description,
            Organic = request.Organic,
            ExpirationPeriodByDays = request.ExpirationPeriodByDays,
            Stock = request.Stock,
            CategoryId = request.CategoryId,
        };

        var imageDto = new ImageDto
        {
            Content = request.Image.OpenReadStream(),
            Length = request.Image.Length,
            ContentType = request.Image.ContentType
        };
        
        var product = await _productService.CreateAsync(adminId, dto, imageDto);
        
        return CreatedAtAction(
            "GetById",
            new { productId = product.Id },
            product);
    }
    
    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="request">Updated product data</param>
    /// <remarks>
    /// Requires admin authorization.  
    /// Content-Type must be <c>multipart/form-data</c>.
    /// </remarks>
    /// <response code="204">Product updated successfully</response>
    /// <response code="400">Invalid update data or image</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User is not authorized (Admin only)</response>
    /// <response code="404">Product or category not found</response>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{productId:int}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] int productId,
        [FromForm] UpdateProductRequest request)
    {
        ImageDto? imageDto = null;

        if (request.Image != null)
        {
            imageDto = new ImageDto
            {
                Content = request.Image.OpenReadStream(),
                Length = request.Image.Length,
                ContentType = request.Image.ContentType
            };
        }

        var dto = new UpdateProductDto
        {
            Name = request.Name,
            Price = request.Price,
            Calories = request.Calories,
            Description = request.Description,
            Organic = request.Organic,
            ExpirationPeriodByDays = request.ExpirationPeriodByDays,
            Stock = request.Stock,
            CategoryId = request.CategoryId
        };

        await _productService.UpdateAsync(productId, dto, imageDto);
        return NoContent();
    }
    
    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <response code="204">Product deleted successfully</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User is not authorized (Admin only)</response>
    /// <response code="404">Product not found</response>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int productId)
    {
        await _productService.DeleteAsync(productId);
        return NoContent();
    }
}