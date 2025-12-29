using System.Security.Claims;
using FruitHub.ApplicationCore.DTOs.Auth.Register;
using FruitHub.ApplicationCore.DTOs.User;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.ApplicationCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserFavoritesController : ControllerBase
{
    private readonly IUserFavoritesService _userFavoritesService;

    public UserFavoritesController(IUserFavoritesService userFavoritesService)
    {
        _userFavoritesService = userFavoritesService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
    
        var favoriteProducts = await _userFavoritesService.GetAsync(userId);
    
        if (!favoriteProducts.Any())
        {
            return NoContent();
        }
        
        return Ok(favoriteProducts);
    }
    
    [HttpPost("{productId:int}")]
    public async Task<IActionResult> AddProductAsync(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
    
        await _userFavoritesService.AddAsync(userId, productId);
        
        return NoContent();
    }
    
    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> RemoveProductAsync(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
    
        await _userFavoritesService.RemoveAsync(userId, productId);
        
        return NoContent();
    }
}