namespace FruitHub.ApplicationCore.DTOs.Order;

public class CheckoutDto
{
    public string CustomerFullName { get; set; } = null!;
    public string CustomerAddress { get; set; } = null!;
    public string CustomerCity { get; set; } = null!;
    public int CustomerDepartment { get; set; } 
    public string CustomerPhoneNumber { get; set; } = null!;
}