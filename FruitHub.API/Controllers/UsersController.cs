using System.Security.Claims;
using FruitHub.ApplicationCore.DTOs.Auth.Register;
using FruitHub.ApplicationCore.DTOs.User;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
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
        var user = await _authService.RegisterAsync(dto);
        
        return CreatedAtAction("GetById", user);
    }

    [Authorize(Roles = "User")]
    [HttpGet("me")]
    public async Task<IActionResult> GetByIdAsync()
    {
        var userId = GetUserId();
        
        var userProfile = await _userService.GetByIdAsync(userId);
        
        return Ok(userProfile);
    }

    [Authorize(Roles = "User")]
    [HttpPatch("me")]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateUserDto dto)
    {
        var userId = GetUserId();
        
        await _userService.UpdateAsync(userId, dto);
        return NoContent();
    }
    
    private int GetUserId()
    {
        var value = User.FindFirstValue("business_user_id");
        return value == null ? throw new UnauthorizedAccessException() : int.Parse(value);
    }
}