using FruitHub.ApplicationCore.Interfaces;

namespace FruitHub.ApplicationCore.Models;

public class Cart : BaseEntity, IEntity<int>
{
    public int Id { get; set; }
    public int UserId { get; set; } 
    public User User { get; set; } = null!;
    public List<CartItem> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(i => i.Quantity * i.Product.Price);
}