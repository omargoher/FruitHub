using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.Category;

public class UpdateCategoryDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = null!;
}