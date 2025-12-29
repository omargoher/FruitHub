using FruitHub.ApplicationCore.Interfaces;

namespace FruitHub.ApplicationCore.Models;

public class User : IEntity<int>
{
    public int Id {get; set;}
    public string UserId { get; set; } = null!;
    public string FullName {get; set;} = null!;
    public string Email {get; set;} = null!;
    public List<Order> Orders { get; set; } = new();
    public List<UserFavorite> FavoriteList { get; set; } = new();
    public Cart Cart { get; set; } = null!;
}