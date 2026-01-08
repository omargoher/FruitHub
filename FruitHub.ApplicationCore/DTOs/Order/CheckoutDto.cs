using System.ComponentModel.DataAnnotations;

namespace FruitHub.ApplicationCore.DTOs.Order;

public class CheckoutDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string CustomerFullName { get; set; } = null!;
    
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string CustomerAddress { get; set; } = null!;
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string CustomerCity { get; set; } = null!;
    
    [Required]
    [Range(0, 100)]
    public int CustomerDepartment { get; set; } 
    
    [Required]
    [Phone]
    [StringLength(20)]
    public string CustomerPhoneNumber { get; set; } = null!;
}