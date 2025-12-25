using FruitHub.ApplicationCore.DTOs.Auth.PasswordRecovery;

namespace FruitHub.ApplicationCore.Interfaces;

public interface IPasswordRecoveryService
{
    Task SendCodeAsync(SendForgetPasswordCodeDto dto);
    Task<VerifyForgetPasswordCodeResponseDto> VerifyCodeAsync(VerifyForgetPasswordCodeDto dto);
    Task<ResetPasswordResponseDto> ResetAsync(ResetPasswordDto dto);
}