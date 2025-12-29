using FruitHub.ApplicationCore.DTOs.Cart;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface ICartService
{
    Task<IReadOnlyList<CartResponseDto>> GetItemsAsync(string userId);
    Task AddItemAsync(string identityUserId, int productId, int quantity);
    Task UpdateQuantityAsync(string identityUserId, int productId, int quantity);
    Task RemoveItemAsync(string identityUserId, int productId);
}