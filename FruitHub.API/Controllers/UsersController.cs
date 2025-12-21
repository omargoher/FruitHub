using FruitHub.ApplicationCore.DTOs.Auth.Register;
using FruitHub.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : Controller
{
    private readonly IAuthService _authService;

    public UsersController(IAuthService authService)
    {
        _authService = authService;
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
}