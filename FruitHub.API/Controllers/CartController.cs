using System.Security.Claims;
using FruitHub.API.Extensions;
using FruitHub.API.Responses;
using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;


/// <summary>
/// Manages the authenticated user's shopping cart.
/// </summary>
/// <remarks>
/// This controller allows users to view and manage items in their cart.
///
/// **Authorization:**
/// - Requires authenticated user with **User** role.
/// </remarks>
/// <response code="403">User is not authorized (User only)</response>
/// <response code="401">User is not authenticated.</response>
[ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status403Forbidden)]
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "User")]
[Tags("Cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Retrieves all items in the user's cart.
    /// </summary>
    /// <remarks>
    /// Returns the current cart including item details and total price.
    /// </remarks>
    /// <returns>User's cart.</returns>
    /// <response code="200">Cart retrieved successfully.</response>
    /// <response code="404">cart not found.</response>
    [HttpGet]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllItemsAsync()
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);
        var cartItems =
            await _cartService.GetAllItemsAsync(userId);
        return Ok(cartItems);
    }

    /// <summary>
    /// Adds a product to the user's cart.
    /// </summary>
    /// <remarks>
    /// If the product already exists in the cart, the quantity is increased.
    ///
    /// **Rules:**
    /// - Quantity must be greater than zero.
    /// - Quantity must not exceed available stock.
    /// </remarks>
    /// <param name="productId">Product identifier.</param>
    /// <param name="dto">Item quantity.</param>
    /// <response code="204">Item added successfully.</response>
    /// <response code="400">Invalid quantity or insufficient stock.</response>
    /// <response code="404">Product not found.</response>
    [HttpPost("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItemAsync(int productId, [FromBody] AddToCartDto dto)
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);
        await _cartService.AddItemAsync(userId, productId, dto.Quantity);
        return NoContent();
    }

    /// <summary>
    /// Updates the quantity of a product in the cart.
    /// </summary>
    /// <remarks>
    /// **Rules:**
    /// - Quantity must be greater than zero.
    /// - Quantity must not exceed available stock.
    /// </remarks>
    /// <param name="productId">Product identifier.</param>
    /// <param name="dto">Updated quantity.</param>
    /// <response code="204">Cart item updated successfully.</response>
    /// <response code="400">Invalid quantity or insufficient stock.</response>
    /// <response code="404">Cart or product not found.</response>
    [HttpPut("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuantityAsync(int productId, [FromBody] UpdateCartItemDto dto)
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);
        await _cartService.UpdateQuantityAsync(userId, productId, dto.Quantity);
        return NoContent();
    }

    /// <summary>
    /// Removes a product from the user's cart.
    /// </summary>
    /// <remarks>
    /// This operation is idempotent.
    /// Removing a non-existing item has no effect.
    /// </remarks>
    /// <param name="productId">Product identifier.</param>
    /// <response code="204">Item removed successfully.</response>
    /// <response code="404">Cart or product not found.</response>
    [HttpDelete("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItemAsync(int productId)
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);
        await _cartService.RemoveItemAsync(userId, productId);
        return NoContent();
    }
}