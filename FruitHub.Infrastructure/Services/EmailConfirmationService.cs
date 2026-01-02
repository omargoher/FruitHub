using FruitHub.ApplicationCore.DTOs;
using FruitHub.ApplicationCore.DTOs.Auth.EmailVerification;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.Infrastructure.Identity;
using FruitHub.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FruitHub.Infrastructure.Services;

public class EmailConfirmationService : IEmailConfirmationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IAppCache _cache;
    private readonly IOtpService _otp;
    
    public EmailConfirmationService(
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
    
    public async Task SendAsync(SendEmailConfirmationCodeDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || user.EmailConfirmed)
            return;

        var otp = _otp.GenerateOtp();
        var cacheKey = $"email-confirm:{dto.Email}";

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
            "Confirm your email",
            $"This is Confirmation Code: <b>{otp}</b>"
        );
    }

    // use exception to handele errors
    public async Task ConfirmAsync(ConfirmEmailCodeDto confirmEmailDto)
    {
        ArgumentNullException.ThrowIfNull(confirmEmailDto);
        
        var cacheKey = $"email-confirm:{confirmEmailDto.Email}";
        var cached = await _cache.GetAsync<EmailOtpCacheModel>(cacheKey);

        if (cached is null)
        {
            throw new InvalidOtp("OTP Is Expired");
        }

        if (cached.AttemptsLeft <= 0)
        {
            throw new InvalidOtp("OTP Is Locked");
        }

        if (cached.Otp != confirmEmailDto.Otp)
        {
            cached.AttemptsLeft--;

            await _cache.SetAsync(
                cacheKey,
                cached,
                TimeSpan.FromMinutes(10)
            );
            throw new InvalidOtp("OTP Is Invalid");
        }

        var user = await _userManager.FindByEmailAsync(confirmEmailDto.Email);
        if (user is null)
        {
            throw new NotFoundException("User not found");
        }

        if (user.EmailConfirmed)
        {
            return;
        }
        
        user.EmailConfirmed = true;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            throw new IdentityOperationException(result.Errors
                .Select(e => e.Description));
        
        await _cache.RemoveAsync(cacheKey);
    }
}