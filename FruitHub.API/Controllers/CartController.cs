using System.Security.Claims;
using FruitHub.API.Extensions;
using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "User")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllItemsAsync()
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);

        var cartItems =
            await _cartService.GetAllItemsAsync(userId);

        return Ok(cartItems);
    }

    [HttpPost("{productId:int}")]
    public async Task<IActionResult> AddItemAsync(int productId, [FromBody] AddToCartDto dto)
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);

        await _cartService.AddItemAsync(userId, productId, dto.Quantity);

        return NoContent();
    }

    [HttpPut("{productId:int}")]
    public async Task<IActionResult> UpdateQuantityAsync(int productId, [FromBody] UpdateCartItemDto dto)
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);

        await _cartService.UpdateQuantityAsync(userId, productId, dto.Quantity);

        return NoContent();
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> RemoveItemAsync(int productId)
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);

        await _cartService.RemoveItemAsync(userId, productId);

        return NoContent();
    }
}