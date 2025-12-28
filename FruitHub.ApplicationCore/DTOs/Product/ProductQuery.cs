using FruitHub.ApplicationCore.Enums;

namespace FruitHub.ApplicationCore.DTOs.Product;

public class ProductQuery
{
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public ProductSortBy? SortBy { get; set; }
    public SortDirection SortDir { get; set; } = SortDirection.Asc;
    public string? Search { get; set; }
}