using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.Auth.PasswordRecovery;

public class CreatePasswordResetRequestDto
{
    [Required, MaxLength(255), EmailAddress]
    public string Email { get; set; } = null!;
}