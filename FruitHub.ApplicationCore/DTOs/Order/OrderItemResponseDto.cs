namespace FruitHub.ApplicationCore.DTOs.Order;

public class OrderItemResponseDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal PricePerPiece { get; set; }
    public int Quantity { get; set; }
}