using FruitHub.ApplicationCore.Interfaces;

namespace FruitHub.ApplicationCore.Models;

public class OrderItem : IEntity<int>
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public decimal PricePerPiece { get; set; }
    public int Quentity { get; set; }
}