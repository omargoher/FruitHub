using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.Category;

public class CreateCategoryDto
{
    [Required]
    public string Name { get; set; } = null!;
}