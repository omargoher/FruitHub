using FruitHub.ApplicationCore.Enums;

namespace FruitHub.ApplicationCore.DTOs.Order;

public class OrderFilter
{
    public OrderStatus? Status { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public int? UserId { get; set; }
}