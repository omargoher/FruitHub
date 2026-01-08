using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.Cart;

public class CartDto
{
    [Required]
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
}