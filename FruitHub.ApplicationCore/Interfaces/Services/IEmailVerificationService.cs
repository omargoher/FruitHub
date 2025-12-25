using FruitHub.ApplicationCore.DTOs.Auth.EmailVerification;

namespace FruitHub.ApplicationCore.Interfaces;

public interface IEmailVerificationService
{
    Task SendConfirmationCodeAsync(SendEmailConfirmationCodeDto dto);
    Task<ConfirmEmailResponseDto> ConfirmAsync(ConfirmEmailCodeDto dto);
}