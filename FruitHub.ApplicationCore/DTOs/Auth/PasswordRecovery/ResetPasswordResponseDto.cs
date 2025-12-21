namespace FruitHub.ApplicationCore.DTOs.Auth.PasswordRecovery;

public class ResetPasswordResponseDto
{
    public List<string> Errors { get; set; } = new();
    public bool IsReset { get; set; }
}