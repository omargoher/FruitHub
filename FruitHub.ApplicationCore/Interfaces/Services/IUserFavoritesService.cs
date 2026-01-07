using FruitHub.ApplicationCore.DTOs.Product;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IUserFavoritesService
{
    Task<IReadOnlyList<ProductResponseDto>> GetAllAsync(int userId);
    Task AddAsync(int userId, int productId);
    Task RemoveAsync(int userId, int productId);
}