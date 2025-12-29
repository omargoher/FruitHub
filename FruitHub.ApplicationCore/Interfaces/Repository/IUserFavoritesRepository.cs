using FruitHub.ApplicationCore.DTOs.Product;

namespace FruitHub.ApplicationCore.Interfaces.Repository;

public interface IUserFavoritesRepository
{
    Task<IReadOnlyList<ProductResponseDto>> GetUserFavoriteListAsync(string userId);
    Task<bool> CheckIfProductExist(int userId, int productId);
    void Add(int userId, int productId);
    void Remove(int userId, int productId);
}