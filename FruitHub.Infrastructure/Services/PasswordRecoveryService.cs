using FruitHub.ApplicationCore.DTOs;
using FruitHub.ApplicationCore.DTOs.Auth.PasswordRecovery;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.Infrastructure.Identity;
using FruitHub.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FruitHub.Infrastructure.Services;

public class PasswordRecoveryService : IPasswordRecoveryService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IAppCache _cache;
    private readonly IOtpService _otp;
    
    public PasswordRecoveryService(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IAppCache cache,
        IOtpService otp)
    {
        _userManager = userManager;
        _emailService = emailService;
        _cache = cache;
        _otp = otp;
    }

    public async Task SendCodeAsync(SendForgetPasswordCodeDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || user.EmailConfirmed)
            return;
        
        var otp = _otp.GenerateOtp();
        var cacheKey = $"forget-password:{dto.Email}";

        var cacheValue = new EmailOtpCacheModel
        {
            Otp = otp,
            AttemptsLeft = 2
        };

        await _cache.SetAsync(
            cacheKey,
            cacheValue,
            TimeSpan.FromMinutes(10)
        );
        
        await _emailService.SendAsync(
            user.Email,
            "Reset Password",
            $"This is Reset Password Code: <b>{otp}</b>"
        );
        
    }

    public async Task<VerifyForgetPasswordCodeResponseDto> VerifyCodeAsync(VerifyForgetPasswordCodeDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var response = new VerifyForgetPasswordCodeResponseDto();
        
        var cacheKey = $"forget-password:{dto.Email}";
        var cached = await _cache.GetAsync<EmailOtpCacheModel>(cacheKey);

        if (cached == null)
        {
            response.Errors.Add("OTP_EXPIRED");
            return response;
        }

        if (cached.AttemptsLeft <= 0)
        {
            response.Errors.Add("OTP_LOCKED");
            return response;
        }

        if (cached.Otp != dto.Otp)
        {
            cached.AttemptsLeft--;
            await _cache.SetAsync(cacheKey, cached, TimeSpan.FromMinutes(10));
            response.Errors.Add("OTP_INVALID");
            return response;
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null)
        {
            response.Errors.Add("INVALID_REQUEST");
            return response;
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        await _cache.SetAsync(
            $"forget-password:verified:{resetToken}",
            dto.Email,
            TimeSpan.FromMinutes(10)
        );

        await _cache.RemoveAsync(cacheKey);

        response.ResetToken = resetToken;
        response.IsVerify = true;
        
        return response;
    }
    
    public async Task<ResetPasswordResponseDto> ResetAsync(ResetPasswordDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var response = new ResetPasswordResponseDto();
        
        var cacheKey = $"forget-password:verified:{dto.ResetToken}";
        var email = await _cache.GetAsync<string>(cacheKey);

        if (email == null)
        {
            response.Errors.Add("INVALID_RESET_TOKEN");
            return response;
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            response.Errors.Add("INVALID_REQUEST");
            return response;
        }

        var result = await _userManager.ResetPasswordAsync(
            user,
            dto.ResetToken,
            dto.NewPassword
        );

        if (!result.Succeeded)
        {
            response.Errors.Add("Password reset failed");
            return response;
        }

        await _cache.RemoveAsync(cacheKey);

        response.IsReset = true;
        return response;
    }

}