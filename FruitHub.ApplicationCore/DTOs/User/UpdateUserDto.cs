using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.User;

public class UpdateUserDto
{
    [MaxLength(100)]
    public string? FullName {get; set;} = null!;
}