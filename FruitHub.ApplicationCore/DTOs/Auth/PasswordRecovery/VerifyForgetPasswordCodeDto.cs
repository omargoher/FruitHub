using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.Auth.PasswordRecovery;

public class VerifyForgetPasswordCodeDto
{
    [Required, MaxLength(255), EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    public string Otp { get; set; } = null!;
}