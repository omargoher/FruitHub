using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Repository;

public interface ICartRepository : IGenericRepository<Cart, int>
{
    Task<IReadOnlyList<CartResponseDto>> GetWithCartItemsAsync(int userId);
    Task<Cart?> GetWithCartItemsAndProductsAsync(int userId);
    Task<bool> CheckIfProductExist(int userId, int productId);
    Task<CartItem?> GetItemAsync(int userId, int productId);
}