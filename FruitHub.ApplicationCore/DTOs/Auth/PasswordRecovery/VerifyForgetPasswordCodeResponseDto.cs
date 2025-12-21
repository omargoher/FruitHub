namespace FruitHub.ApplicationCore.DTOs.Auth.PasswordRecovery;

public class VerifyForgetPasswordCodeResponseDto
{
    public string ResetToken { get; set; } = null!;
    public List<string> Errors { get; set; } = new();
    public bool IsVerify { get; set; }
}