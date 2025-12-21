namespace FruitHub.ApplicationCore.DTOs.Auth.Refresh;

public class RefreshTokenDto
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}