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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IAppCache _cache;
    private readonly IOtpService _otp;
    
    public PasswordResetService(
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

    // This the same method in EmailConfirmarion service (generalize it ?)
    public async Task CreateAsync(CreatePasswordResetRequestDto dto)
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

    public async Task<VerifyPasswordResetCodeResponseDto> VerifyAsync(VerifyPasswordResetCodeDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var response = new VerifyPasswordResetCodeResponseDto();
        
        var cacheKey = $"forget-password:{dto.Email}";
        var cached = await _cache.GetAsync<EmailOtpCacheModel>(cacheKey);

        if (cached == null)
        {
            throw new InvalidOtp("OTP Is Expired");
        }

        if (cached.AttemptsLeft <= 0)
        {
            throw new InvalidOtp("OTP Is Locked");
        }

        if (cached.Otp != dto.Otp)
        {
            cached.AttemptsLeft--;

            await _cache.SetAsync(
                cacheKey,
                cached,
                TimeSpan.FromMinutes(10)
            );
            throw new InvalidOtp("OTP Is Invalid");
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            throw new NotFoundException("User not found");
        }
        

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        await _cache.SetAsync(
            $"forget-password:verified:{resetToken}",
            dto.Email,
            TimeSpan.FromMinutes(10)
        );

        await _cache.RemoveAsync(cacheKey);

        response.ResetToken = resetToken;
        
        return response;
    }
    
    public async Task ResetAsync(ResetPasswordDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        
        var cacheKey = $"forget-password:verified:{dto.ResetToken}";
        var email = await _cache.GetAsync<string>(cacheKey);

        if (email == null)
        {
            throw new InvalidResetPasswordToken();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
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

        await _cache.RemoveAsync(cacheKey);
    }

}