using System.Security.Claims;
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
        var userId = GetUserId();
    
        var cartItems = 
            await _cartService.GetAllItemsAsync(userId);
    
        return Ok(cartItems);
    }
    
    [HttpPost("{productId:int}")]
    public async Task<IActionResult> AddItemAsync(int productId, [FromBody] CartDto dto)
    {
        var userId = GetUserId();
    
        await _cartService.AddItemAsync(userId, productId, dto.Quantity);
        
        return NoContent();
    }
    
    [HttpPut("{productId:int}")]
    public async Task<IActionResult> UpdateQuantityAsync(int productId, [FromBody] CartDto dto)
    {
        var userId = GetUserId();
    
        await _cartService.UpdateQuantityAsync(userId, productId, dto.Quantity);
        
        return NoContent();
    }
    
    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> RemoveItemAsync(int productId)
    {
        var userId = GetUserId();
    
        await _cartService.RemoveItemAsync(userId, productId);
        
        return NoContent();
    }
    
    private int GetUserId()
    {
        var value = User.FindFirstValue("business_user_id");
        return value == null ? throw new UnauthorizedAccessException() : int.Parse(value);
    }
}