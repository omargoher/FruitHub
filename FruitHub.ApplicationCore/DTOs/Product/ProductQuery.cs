using System.ComponentModel.DataAnnotations;
using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Enums.Product;

namespace FruitHub.ApplicationCore.DTOs.Product;

/// <summary>
/// Query parameters used to filter, sort, and paginate products
/// </summary>
public class ProductQuery
{
    /// <summary>
    /// Maximum number of products to return.
    /// </summary>
    /// <remarks>
    /// Allowed range: 1–100.
    /// </remarks>
    /// <example>20</example>
    [Range(1, 100)]
    public int? Limit { get; set; }
    
    /// <summary>
    /// Number of products to skip before starting to return results.
    /// </summary>
    /// <remarks>
    /// Used for pagination.
    /// </remarks>
    /// <example>0</example>
    [Range(0, int.MaxValue)]
    public int? Offset { get; set; }
    
    /// <summary>
    /// Field used to sort the products.
    /// </summary>
    /// <remarks>
    /// Possible values: Name, Price, MostSelling, ExpirationPeriod, Calories, CreatedAt.
    /// </remarks>
    /// <example>Name</example>
    [EnumDataType(typeof(ProductSortBy))]
    public ProductSortBy? SortBy { get; set; }
    
    /// <summary>
    /// Sort direction.
    /// </summary>
    /// <remarks>
    /// Asc = ascending order, Desc = descending order.
    /// </remarks>
    /// <example>Asc</example>
    [EnumDataType(typeof(SortDirection))]
    public SortDirection SortDir { get; set; } = SortDirection.Asc;
    
    /// <summary>
    /// Search term used to filter products by name or description.
    /// </summary>
    /// <remarks>
    /// Case-insensitive partial match.
    /// </remarks>
    /// <example>apple</example>
    [MaxLength(100)]
    public string? Search { get; set; }
}