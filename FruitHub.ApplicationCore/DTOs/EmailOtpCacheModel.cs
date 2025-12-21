namespace FruitHub.ApplicationCore.DTOs;

public class EmailOtpCacheModel
{
    public string Otp { get; set; } = null!;
    public int AttemptsLeft { get; set; }
}