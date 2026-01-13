using System.ComponentModel.DataAnnotations;

namespace FruitHub.API.Requests;

/// <summary>
/// Request model for updating an existing product.
/// All fields are optional.
/// </summary>
public class UpdateProductRequest
{
    [StringLength(100, MinimumLength = 3)]
    public string? Name { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? Calories { get; set; }
    
    [StringLength(500, MinimumLength = 10)]
    public string? Description { get; set; }
    
    public bool? Organic { get; set; }

    /// <summary>
    /// Updated expiration period in days.
    /// </summary>
    /// <remarks>
    /// Allowed range: 1–30 days.
    /// </remarks>
    [Range(1, 30)] // up to 30 days
    public int? ExpirationPeriodByDays { get; set; }

    [Range(0, int.MaxValue)]
    public int? Stock { get; set; }
    
    /// <summary>
    /// Updated category identifier.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? CategoryId { get; set; }
    
    /// <summary>
    /// New product image file (optional).
    /// </summary>
    public IFormFile? Image { get; set; }
}