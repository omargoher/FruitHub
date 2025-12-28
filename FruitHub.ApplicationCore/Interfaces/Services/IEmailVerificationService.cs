using FruitHub.ApplicationCore.DTOs.Auth.EmailVerification;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IEmailVerificationService
{
    Task SendConfirmationCodeAsync(SendEmailConfirmationCodeDto dto);
    Task<ConfirmEmailResponseDto> ConfirmAsync(ConfirmEmailCodeDto dto);
}