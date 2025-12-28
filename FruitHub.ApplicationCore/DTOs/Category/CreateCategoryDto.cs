using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.Category;

public class CreateCategoryDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = null!;
}