using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.Auth.PasswordRecovery;

public class SendForgetPasswordCodeDto
{
    [Required, MaxLength(255), EmailAddress]
    public string Email { get; set; } = null!;
}