using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.Auth.EmailVerification;

public class ConfirmEmailCodeDto
{
    [Required, MaxLength(255), EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Otp { get; set; } = null!;
}