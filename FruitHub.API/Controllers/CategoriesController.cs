using System.Collections.Immutable;
using FruitHub.API.Responses;
using FruitHub.ApplicationCore.DTOs.Category;
using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

/// <summary>
/// Manages product categories.
/// </summary>
/// <remarks>
/// Categories are used to organize products in the system.
///
/// **Access Rules:**
/// - Public: Read-only operations
/// - Admin: Create, update, and delete categories
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Categories")]
public class CategoriesController : ControllerBase
{
    private readonly  ICategoryService _categoryService;
    private readonly  IProductService _productService;

    public CategoriesController(ICategoryService categoryService, IProductService productService)
    {
        _categoryService = categoryService;
        _productService = productService;
    }
    
    /// <summary>
    /// Retrieves all available categories.
    /// </summary>
    /// <remarks>
    /// Returns a list of categories without pagination.
    /// </remarks>
    /// <returns>List of categories.</returns>
    /// <response code="200">Categories retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var categories = await _categoryService
            .GetAllAsync();
        return Ok(categories);
    }
    
    /// <summary>
    /// Retrieves a category by its identifier.
    /// </summary>
    /// <param name="categoryId">Category identifier.</param>
    /// <returns>Category details.</returns>
    /// <response code="200">Category retrieved successfully.</response>
    /// <response code="404">Category not found.</response>
    [HttpGet("{categoryId:int}")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int categoryId)
    {
        var category = await _categoryService
            .GetByIdAsync(categoryId);
        return Ok(category);
    }
    
    /// <summary>
    /// Retrieves products belonging to a specific category.
    /// </summary>
    /// <remarks>
    /// Supports filtering, sorting, and pagination using product query parameters.
    /// </remarks>
    /// <param name="categoryId">Category identifier.</param>
    /// <param name="productQuery">Product query parameters.</param>
    /// <returns>List of products in the category.</returns>
    /// <response code="200">Products retrieved successfully.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="400">Invalid query data</response>
    [HttpGet("{categoryId:int}/products")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductsAsync(int categoryId, [FromQuery]ProductQuery productQuery)
    {
        var products = await _productService
            .GetByCategoryAsync(categoryId, productQuery);
       
        return Ok(products);
    }
    
    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <remarks>
    /// **Authorization:**
    /// - Requires **Admin** role.
    /// </remarks>
    /// <param name="dto">Category creation data.</param>
    /// <returns>Created category.</returns>
    /// <response code="201">Category created successfully.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="409">Category name already exists.</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User is not authorized (Admin only)</response>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync([FromBody]CategoryDto dto)
    {
        var category = await _categoryService.CreateAsync(dto);
        return CreatedAtAction(
            "GetById",
            new { categoryId = category.Id },
            category);
    }
    
    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <remarks>
    /// **Authorization:**
    /// - Requires **Admin** role.
    /// </remarks>
    /// <param name="categoryId">Category identifier.</param>
    /// <param name="dto">Updated category data.</param>
    /// <response code="204">Category updated successfully.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="409">Category name already exists.</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User is not authorized (Admin only)</response>
    [Authorize(Roles = "Admin")]
    [HttpPut("{categoryId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute]int categoryId, 
        [FromBody]CategoryDto dto)
    {
        await _categoryService.UpdateAsync(categoryId, dto);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes a category.
    /// </summary>
    /// <remarks>
    /// **Authorization:**
    /// - Requires **Admin** role.
    /// </remarks>
    /// <param name="categoryId">Category identifier.</param>
    /// <response code="204">Category deleted successfully.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User is not authorized (Admin only)</response>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{categoryId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAsync([FromRoute] int categoryId)
    {
        await _categoryService.DeleteAsync(categoryId);
        return NoContent();
    }
}