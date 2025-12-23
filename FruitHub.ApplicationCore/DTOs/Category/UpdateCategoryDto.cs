using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.Category;

public class UpdateCategoryDto
{
    [Required]
    public int Id { get; set; }

    [Required] 
    public string Name { get; set; } = null!;
}