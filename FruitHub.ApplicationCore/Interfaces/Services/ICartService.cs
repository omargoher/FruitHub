using FruitHub.ApplicationCore.DTOs.Cart;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface ICartService
{
    Task<IReadOnlyList<CartResponseDto>> GetAllItemsAsync(int userId);
    Task AddItemAsync(int userId, int productId, int quantity);
    Task UpdateQuantityAsync(int userId, int productId, int quantity);
    Task RemoveItemAsync(int userId, int productId);
}