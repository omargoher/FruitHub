using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.DTOs.Cart;

public class CartResponseDto
{
    public IReadOnlyList<CartItemResponseDto> Items { get; set; } = [];
    public decimal TotalPrice { get; set; }
}