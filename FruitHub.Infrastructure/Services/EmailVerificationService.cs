using FruitHub.ApplicationCore.DTOs;
using FruitHub.ApplicationCore.DTOs.Auth.EmailVerification;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.Infrastructure.Identity;
using FruitHub.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FruitHub.Infrastructure.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IAppCache _cache;
    private readonly IOtpService _otp;
    
    public EmailVerificationService(
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
    
    public async Task SendConfirmationCodeAsync(SendEmailConfirmationCodeDto dto)
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

    public async Task<ConfirmEmailResponseDto> ConfirmAsync(ConfirmEmailCodeDto confirmEmailDto)
    {
        ArgumentNullException.ThrowIfNull(confirmEmailDto);
        var response = new ConfirmEmailResponseDto();
        
        var cacheKey = $"email-confirm:{confirmEmailDto.Email}";
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

        if (cached.Otp != confirmEmailDto.Otp)
        {
            cached.AttemptsLeft--;
            await _cache.SetAsync(cacheKey, cached, TimeSpan.FromMinutes(10));
            response.Errors.Add("OTP_INVALID");
            return response;
        }

        var user = await _userManager.FindByEmailAsync(confirmEmailDto.Email);

        if (user == null)
        {
            response.Errors.Add("INVALID_REQUEST");
            return response;
        }

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        await _cache.RemoveAsync(cacheKey);
        response.IsConfirmed = true;

        return response;
    }
}