using FruitHub.ApplicationCore.DTOs.Product;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IUserFavoritesService
{
    Task<IReadOnlyList<ProductResponseDto>> GetAsync(string identityUserId);
    Task AddAsync(string identityUserId, int productId);
    Task RemoveAsync(string identityUserId, int productId);
}