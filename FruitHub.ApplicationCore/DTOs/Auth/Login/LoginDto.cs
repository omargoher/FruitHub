using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.Auth.Login;

public class LoginDto
{
    [Required, MaxLength(255), EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
}