using FruitHub.ApplicationCore.Enums.Order;

namespace FruitHub.ApplicationCore.DTOs.Order;

public class UpdateOrderStatusDto
{
    public OrderStatus OrderStatus { get; set; }
}