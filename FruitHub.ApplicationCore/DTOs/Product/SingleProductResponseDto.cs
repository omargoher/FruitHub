namespace FruitHub.ApplicationCore.DTOs.Product;

public class SingleProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int Calories { get; set; }
    public string Description { get; set; } = null!;
    public bool Organic { get; set; }
    public int ExpirationPeriodByDays { get; set; }
    public string ImagePath { get; set; } = null!;
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
}