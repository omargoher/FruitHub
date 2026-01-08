namespace FruitHub.ApplicationCore.DTOs.Order;

public class ChangeOrderStatusDto
{
    public bool? IsShipped { get; set; }
    public bool? IsPayed { get; set; }
}