namespace FruitHub.ApplicationCore.DTOs.Auth.PasswordRecovery;

public class VerifyPasswordResetCodeResponseDto
{
    public string ResetToken { get; set; } = null!;
}