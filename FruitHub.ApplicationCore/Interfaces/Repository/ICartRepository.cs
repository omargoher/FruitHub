using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Repository;

public interface ICartRepository : IGenericRepository<Cart, int>
{
    Task<IReadOnlyList<CartResponseDto>> GetByUserIdWithCartItemsAsync(int userId);
    Task<Cart?> GetByUserIdWithCartItemsAndProductsAsync(int userId);
}