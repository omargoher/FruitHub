using System.ComponentModel.DataAnnotations;

namespace FruitHub.API.Requests;

/// <summary>
/// Request model for creating a new product with image upload
/// </summary>
public class CreateProductRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = null!;
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Calories { get; set; }
    
    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Description { get; set; } = null!;
    
    [Required]
    public bool Organic { get; set; }
    
    /// <summary>
    /// Number of days before expires.
    /// </summary>
    /// <remarks>
    /// Allowed range: 1–30 days.
    /// </remarks>
    [Required]
    [Range(1, 30)] // up to 30 days
    public int ExpirationPeriodByDays { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    
    /// <summary>
    /// Identifier of the category this product belongs to.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    /// <summary>
    /// Product image file.
    /// </summary>
    /// <remarks>
    /// Supported formats and size are validated server-side.
    /// </remarks>
    [Required] 
    public IFormFile Image { get; set; } = null!;
}