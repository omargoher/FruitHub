using FruitHub.ApplicationCore.Enums.Order;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;

namespace FruitHub.ApplicationCore.Models;

public class Order : BaseEntity, IEntity<int>
{
    public int Id { get; set; }
    public string CustomerFullName { get; set; } = null!;
    public string CustomerAddress { get; set; } = null!;
    public string CustomerCity { get; set; } = null!;
    public int CustomerDepartment { get; set; } 
    public string CustomerPhoneNumber { get; set; } = null!;
    public decimal SubPrice { get; set; } 
    public decimal TotalPrice { get; set; }
    public decimal ShippingFees { get; set; }
    public OrderStatus OrderStatus { get; private set; } 
    public List<OrderItem> Items { get; set; } = new();
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public void ChangeStatus(OrderStatus newStatus)
    {
        if (!IsValidTransition(newStatus))
            throw new DomainException(
                $"Invalid order status change: {OrderStatus} → {newStatus}"
            );

        OrderStatus = newStatus;
    }
    
    public void Cancel()
    {
        if (OrderStatus != OrderStatus.Pending)
            throw new DomainException(
                $"Order cannot be cancelled when status is {OrderStatus}"
            );

        OrderStatus = OrderStatus.Cancelled;
    }
    
    private bool IsValidTransition(OrderStatus target)
    {
        return OrderStatus switch
        {
            OrderStatus.Pending =>
                target == OrderStatus.Shipped ||
                target == OrderStatus.Cancelled,

            OrderStatus.Shipped =>
                target == OrderStatus.Delivered,

            _ => false
        };
    }
}