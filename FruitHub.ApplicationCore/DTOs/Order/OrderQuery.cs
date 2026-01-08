using System.ComponentModel.DataAnnotations;
using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Enums.Order;
using FruitHub.ApplicationCore.Enums.Product;

namespace FruitHub.ApplicationCore.DTOs.Order;

public class OrderQuery
{
    [Range(1, 100)]
    public int? Limit { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? Offset { get; set; }
    
    public OrderFilterDto? Filter { get; set; }

    [EnumDataType(typeof(ProductSortBy))]
    public OrderSortBy? SortBy { get; set; }
    
    [EnumDataType(typeof(SortDirection))]
    public SortDirection SortDir { get; set; } = SortDirection.Asc;
}