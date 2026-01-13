using FruitHub.ApplicationCore.Enums.Order;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.DTOs.Order;

public class OrderResponseDto
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string CustomerFullName { get; set; } = null!;
    public string CustomerAddress { get; set; } = null!;
    public string CustomerCity { get; set; } = null!;
    public int CustomerDepartment { get; set; } 
    public string CustomerPhoneNumber { get; set; } = null!;
    public decimal SubPrice { get; set; } 
    public decimal TotalPrice { get; set; }
    public decimal ShippingFees { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public IReadOnlyList<OrderItemResponseDto> Items { get; set; } = [];
}
