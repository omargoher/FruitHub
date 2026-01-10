using System.ComponentModel.DataAnnotations;
using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Enums.Order;
using FruitHub.ApplicationCore.Enums.Product;

namespace FruitHub.ApplicationCore.DTOs.Order;

/// <summary>
/// Query parameters used to filter, sort, and paginate orders.
/// </summary>
public class OrderQuery
{
    /// <summary>
    /// Maximum number of orders to return.
    /// </summary>
    /// <remarks>
    /// Allowed range: 1–100.
    /// </remarks>
    /// <example>20</example>
    [Range(1, 100)]
    public int? Limit { get; set; }
    
    /// <summary>
    /// Number of orders to skip before starting to return results.
    /// </summary>
    /// <remarks>
    /// Used for pagination.
    /// </remarks>
    /// <example>0</example>
    [Range(0, int.MaxValue)]
    public int? Offset { get; set; }
    
    /// <summary>
    /// Order filtering options.
    /// </summary>
    /// <remarks>
    /// Allows filtering orders by status, payment, shipping, or date.
    /// </remarks>
    public OrderFilterDto? Filter { get; set; }

    /// <summary>
    /// Field used to sort the orders.
    /// </summary>
    /// <remarks>
    /// Possible values depend on <see cref="OrderSortBy"/> enum
    /// (e.g., CreatedAt, TotalPrice, Status).
    /// </remarks>
    /// <example>CreatedAt</example>
    [EnumDataType(typeof(ProductSortBy))]
    public OrderSortBy? SortBy { get; set; }
    
    /// <summary>
    /// Sort direction.
    /// </summary>
    /// <remarks>
    /// Asc = ascending order, Desc = descending order.
    /// </remarks>
    /// <example>Desc</example>
    [EnumDataType(typeof(SortDirection))]
    public SortDirection SortDir { get; set; } = SortDirection.Asc;
}