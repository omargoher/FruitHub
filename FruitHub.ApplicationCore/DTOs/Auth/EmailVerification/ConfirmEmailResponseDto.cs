namespace FruitHub.ApplicationCore.DTOs.Auth.EmailVerification;

public class ConfirmEmailResponseDto
{
    public List<string> Errors { get; set; } = new();
    public bool IsConfirmed { get; set; }
}