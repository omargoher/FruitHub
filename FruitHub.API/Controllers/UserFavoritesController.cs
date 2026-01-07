using System.Security.Claims;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize(Roles = "User")]
public class UserFavoritesController : ControllerBase
{
    private readonly IUserFavoritesService _userFavoritesService;

    public UserFavoritesController(IUserFavoritesService userFavoritesService)
    {
        _userFavoritesService = userFavoritesService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var userId = GetUserId();
    
        var favoriteProducts = await _userFavoritesService
            .GetAllAsync(userId);
        
        return Ok(favoriteProducts);
    }
    
    [HttpPost("{productId:int}")]
    public async Task<IActionResult> AddProductAsync(int productId)
    {
        var userId = GetUserId();
    
        await _userFavoritesService.AddAsync(userId, productId);
        
        return NoContent();
    }
    
    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> RemoveProductAsync(int productId)
    {
        var userId = GetUserId();
    
        await _userFavoritesService.RemoveAsync(userId, productId);
        
        return NoContent();
    }
    
    private int GetUserId()
    {
        var value = User.FindFirstValue("business_user_id");
        return value == null ? throw new UnauthorizedAccessException() : int.Parse(value);
    }
}