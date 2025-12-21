using FruitHub.ApplicationCore.Interfaces;

namespace FruitHub.ApplicationCore.Models;

public class CartItem : IEntity<int>
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quentity { get; set; }
    public int CartId { get; set; }
    public Cart Cart { get; set; } = null!;
}