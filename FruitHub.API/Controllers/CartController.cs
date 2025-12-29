using System.Security.Claims;
using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
    
        var cartItems = await _cartService.GetItemsAsync(userId);
    
        if (!cartItems.Any())
        {
            return NoContent();
        }
        
        return Ok(cartItems);
    }
    
    [HttpPost("{productId:int}")]
    public async Task<IActionResult> AddItemAsync(int productId, [FromBody] AddItemToCartDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
    
        await _cartService.AddItemAsync(userId, productId, dto.Quantity);
        
        return NoContent();
    }
    
    [HttpPut("{productId:int}")]
    public async Task<IActionResult> UpdateQuantityAsync(int productId, [FromBody] UpdateQuantityDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
    
        await _cartService.UpdateQuantityAsync(userId, productId, dto.Quantity);
        
        return NoContent();
    }
    
    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> RemoveItemAsync(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
    
        await _cartService.RemoveItemAsync(userId, productId);
        
        return NoContent();
    }
}