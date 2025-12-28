using System.Security.Claims;
using FruitHub.ApplicationCore.DTOs.Auth.Register;
using FruitHub.ApplicationCore.DTOs.User;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public UsersController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }
    
    [HttpPost]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto dto)
    {

        var response = await _authService.RegisterAsync(dto);
        
        if (!response.IsRegistered)
        {
            return BadRequest(
                new
                {
                    Errors = response.Errors
                });
        }
        
        return Ok(new {
            response.FullName,
            response.Email,
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
        
        var userProfile = await _userService.GetUserAsync(userId);
        
        return Ok(userProfile);
    }

    [Authorize]
    [HttpPatch("me")]
    public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        await _userService.UpdateUserAsync(userId, dto);
        return NoContent();
    }
}