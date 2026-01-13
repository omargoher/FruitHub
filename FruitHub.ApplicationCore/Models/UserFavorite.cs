using FruitHub.ApplicationCore.Interfaces;

namespace FruitHub.ApplicationCore.Models;

public class UserFavorite : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}