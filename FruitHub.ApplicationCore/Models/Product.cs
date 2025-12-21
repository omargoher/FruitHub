using FruitHub.ApplicationCore.Interfaces;

namespace FruitHub.ApplicationCore.Models;

public class Product : IEntity<int>
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
    public Category Category { get; set; } = null!;
    public int AdminId { get; set; }
    public Admin Admin { get; set; } = null!;
}