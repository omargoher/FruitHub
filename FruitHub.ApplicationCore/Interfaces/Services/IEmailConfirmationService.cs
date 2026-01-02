using FruitHub.ApplicationCore.DTOs.Auth.EmailVerification;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IEmailConfirmationService
{
    Task SendAsync(SendEmailConfirmationCodeDto dto);
    Task ConfirmAsync(ConfirmEmailCodeDto dto);
}