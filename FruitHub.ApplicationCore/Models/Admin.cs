using FruitHub.ApplicationCore.Interfaces;

namespace FruitHub.ApplicationCore.Models;

public class Admin : IEntity<int>
{
    public int Id {get; set;}
    public string UserId {get; set;}
    public string FullName {get; set;}
    public string Email {get; set;}
    public List<Product> Products { get; set; } = new();
}