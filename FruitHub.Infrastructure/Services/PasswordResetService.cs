using FruitHub.ApplicationCore.DTOs;
using FruitHub.ApplicationCore.DTOs.Auth.PasswordRecovery;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.Infrastructure.Identity;
using FruitHub.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FruitHub.Infrastructure.Services;

public class PasswordResetService : IPasswordResetService
{
    private const string ForgetPasswordKey = "forget-password";
    private const string ForgetPasswordVerifyKey = "forget-password:verified";
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otp;
    private readonly IAppCache _cache;
    
    public PasswordResetService(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IOtpService otp,
        IAppCache cache
        )
    {
        _userManager = userManager;
        _emailService = emailService;
        _otp = otp;
        _cache = cache;
    }

    public async Task CreateAsync(CreatePasswordResetRequestDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null)
        {
            return;
        }
        
        var key = $"{ForgetPasswordKey}:{dto.Email}";
        
        var otp = await _otp.CreateOtpAsync(key);

        await _emailService.SendAsync(
            user.Email,
            "Reset Password",
            $"This is Reset Password Code: <b>{otp}</b>"
        );
    }

    public async Task<VerifyPasswordResetCodeResponseDto> VerifyAsync(VerifyPasswordResetCodeDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            // good return not found when user is null ?!
            throw new NotFoundException("User not found");
        }
        
        var key = $"{ForgetPasswordKey}:{dto.Email}";
        await _otp.VerifyOtpAsync(key, dto.Otp);

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        await _cache.SetAsync(
            $"{ForgetPasswordVerifyKey}:{resetToken}",
            dto.Email,
            TimeSpan.FromMinutes(10)
        );

        await _otp.RemoveOtpAsync(key);

        return new VerifyPasswordResetCodeResponseDto
        {
            ResetToken = resetToken
        };
    }
    
    public async Task ResetAsync(ResetPasswordDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        
        var key = $"{ForgetPasswordVerifyKey}:{dto.ResetToken}";
        var email = await _cache.GetAsync<string>(key);

        if (email == null)
        {
            throw new InvalidResetPasswordToken();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var result = await _userManager.ResetPasswordAsync(
            user,
            dto.ResetToken,
            dto.NewPassword
        );

        if (!result.Succeeded)
        {
            throw new IdentityOperationException(result.Errors.Select(e => e.Description));
        }

        await _cache.RemoveAsync(key);
    }
}