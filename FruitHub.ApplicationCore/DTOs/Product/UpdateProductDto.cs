using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.Product;

public class UpdateProductDto
{
    public int Id { get; set; }
    
    [StringLength(100, MinimumLength = 3)]
    public string? Name { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public int? Calories { get; set; }
    
    [StringLength(500, MinimumLength = 10)]
    public string? Description { get; set; }
    
    public bool? Organic { get; set; }

    [Range(1, 30)] // up to 30 days
    public int? ExpirationPeriodByDays { get; set; }

    [Range(0, int.MaxValue)]
    public int? Stock { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? CategoryId { get; set; }
}
