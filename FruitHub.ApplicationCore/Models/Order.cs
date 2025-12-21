using FruitHub.ApplicationCore.Interfaces;

namespace FruitHub.ApplicationCore.Models;

public class Order : IEntity<int>
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
    public bool IsShipped { get; set; }
    public bool IsPayed { get; set; }
    public List<OrderItem> Items { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}