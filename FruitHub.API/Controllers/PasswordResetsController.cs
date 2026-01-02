using FruitHub.ApplicationCore.DTOs.Auth.EmailVerification;
using FruitHub.ApplicationCore.DTOs.Auth.PasswordRecovery;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

[ApiController]
[Route("api/password-resets")]
public class PasswordResetsController : ControllerBase
{
    private readonly IPasswordResetService _passwordResetService;

    public PasswordResetsController(IPasswordResetService passwordResetService)
    {
        _passwordResetService = passwordResetService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreatePasswordResetRequestDto dto)
    {
        await _passwordResetService.CreateAsync(dto);
        return Accepted();
    }
 
    [HttpPut("verify")]
    public async Task<IActionResult> VerifyAsync(VerifyPasswordResetCodeDto dto)
    {
        var token = await _passwordResetService.VerifyAsync(dto);
        return Ok(token);
    }
    
    [HttpPut]
    public async Task<IActionResult> ResetAsync(ResetPasswordDto dto)
    {
        await _passwordResetService.ResetAsync(dto);
        return NoContent();
    }
}