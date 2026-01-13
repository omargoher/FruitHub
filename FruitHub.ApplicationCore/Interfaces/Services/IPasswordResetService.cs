using FruitHub.ApplicationCore.DTOs.Auth.PasswordRecovery;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IPasswordResetService
{
    Task CreateAsync(CreatePasswordResetRequestDto dto);
    Task<VerifyPasswordResetCodeResponseDto> VerifyAsync(VerifyPasswordResetCodeDto dto);
    Task ResetAsync(ResetPasswordDto dto);
}