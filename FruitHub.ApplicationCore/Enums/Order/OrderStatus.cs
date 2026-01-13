namespace FruitHub.ApplicationCore.Enums.Order;

public enum OrderStatus
{
    Pending   = 0, // Order created, not paid
    Shipped   = 1, // Order handed to delivery
    Delivered = 2, // Customer received order
    Cancelled = 3  // Order cancelled (before shipping)
}