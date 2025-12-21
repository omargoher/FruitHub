namespace FruitHub.ApplicationCore.DTOs.Auth.Login;

public class LoginResponseDto
{
    public string? Email { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public DateTime? RefreshExpiresAt { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool IsAuthenticated { get; set; }
}