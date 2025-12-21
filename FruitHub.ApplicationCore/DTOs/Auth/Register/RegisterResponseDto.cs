namespace FruitHub.ApplicationCore.DTOs.Auth.Register;

public class RegisterResponseDto
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool IsRegistered { get; set; }
}