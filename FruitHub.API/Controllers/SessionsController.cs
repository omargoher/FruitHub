using System.Security.Claims;
using FruitHub.ApplicationCore.DTOs.Auth.Login;
using FruitHub.ApplicationCore.DTOs.Auth.Refresh;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : Controller
{
    private readonly IAuthService _authService;

    public SessionsController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto)
    {
        var response = await _authService.LoginAsync(dto);
        
        if (!response.IsAuthenticated)
        {
            //ProblemDetails
            return BadRequest(
                new
                {
                    Errors = response.Errors
                });
        }
        
        return Created(string.Empty, new {
            response.Email,
            response.Token,
            response.TokenExpiresAt,
            response.RefreshToken,
            response.RefreshExpiresAt,
        });
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequestDto refreshToken)
    {
        var response = await _authService.RefreshAsync(refreshToken);
        
        if (!response.IsAuthenticated)
        {
            return BadRequest(
                new
                {
                    Errors = response.Errors
                });
        }
        
        return Ok(new {
            response.Email,
            response.Token,
            response.TokenExpiresAt,
            response.RefreshToken,
            response.RefreshExpiresAt,
        });
    }
    
    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> LogoutAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _authService.LogoutAsync(userId);
        
        return NoContent();
    }
}