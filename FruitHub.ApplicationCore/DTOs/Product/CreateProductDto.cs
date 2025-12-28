using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.Product;

public class CreateProductDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = null!;
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public int Calories { get; set; }
    
    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Description { get; set; } = null!;
    
    [Required]
    public bool Organic { get; set; }
    
    [Required]
    [Range(1, 30)] // up to 30 days
    public int ExpirationPeriodByDays { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}