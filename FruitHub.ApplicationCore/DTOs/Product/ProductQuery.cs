using System.ComponentModel.DataAnnotations;
using FruitHub.ApplicationCore.Enums;

namespace FruitHub.ApplicationCore.DTOs.Product;

public class ProductQuery
{
    [Range(1, 100)]
    public int? Limit { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? Offset { get; set; }
    
    [EnumDataType(typeof(ProductSortBy))]
    public ProductSortBy? SortBy { get; set; }
    
    [EnumDataType(typeof(SortDirection))]
    public SortDirection SortDir { get; set; } = SortDirection.Asc;
    
    [MaxLength(100)]
    public string? Search { get; set; }
}