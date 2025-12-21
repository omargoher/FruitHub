using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.Auth.Refresh;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
