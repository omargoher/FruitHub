using System.Security.Claims;
using FruitHub.API.Extensions;
using FruitHub.API.Responses;
using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

/// <summary>
/// Manage the authenticated user's favorite products
/// </summary>
/// <response code="401">User is not authenticated.</response>
/// <response code="403">User is not authorized (User only)</response>
[ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status403Forbidden)]
[ApiController]
[Route("api/favorites")]
[Authorize(Roles = "User")]
[Tags("UserFavorites")]
public class UserFavoritesController : ControllerBase
{
    private readonly IUserFavoritesService _userFavoritesService;

    public UserFavoritesController(IUserFavoritesService userFavoritesService)
    {
        _userFavoritesService = userFavoritesService;
    }
    
    /// <summary>
    /// Get all favorite products for the authenticated user
    /// </summary>
    /// <returns>List of favorite products</returns>
    /// <response code="200">Favorites retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);
    
        var favoriteProducts = await _userFavoritesService
            .GetAllAsync(userId);
        
        return Ok(favoriteProducts);
    }
    
    /// <summary>
    /// Add a product to the authenticated user's favorites
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <response code="204">Product added to favorites successfully</response>
    /// <response code="404">product not found</response>
    [HttpPost("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddProductAsync(int productId)
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);
        await _userFavoritesService.AddAsync(userId, productId);
        return NoContent();
    }
    
    /// <summary>
    /// Remove a product from the authenticated user's favorites
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <response code="204">Product removed from favorites successfully</response>
    /// <response code="404">product not found</response>
    [HttpDelete("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveProductAsync(int productId)
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);
        await _userFavoritesService.RemoveAsync(userId, productId);
        return NoContent();
    }
}