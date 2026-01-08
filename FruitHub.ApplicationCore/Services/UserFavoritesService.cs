using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Interfaces.Services;

namespace FruitHub.ApplicationCore.Services;

public class UserFavoritesService : IUserFavoritesService
{
    private readonly IUnitOfWork _uow;
    private readonly IUserFavoritesRepository _userFavoritesRepo;
    private readonly IUserRepository _userRepo;
    private readonly IProductRepository _productRepo;

    public UserFavoritesService(IUnitOfWork uow)
    {
        _uow = uow;
        _userFavoritesRepo = uow.UserFavorites;
        _userRepo = uow.User;
        _productRepo = uow.Product;
    }

    public async Task<IReadOnlyList<ProductResponseDto>> GetAllAsync(int userId)
    {
        var products = await _userFavoritesRepo
            .GetByUserIdAsync(userId);
        return products;
    }

    public async Task AddAsync(int userId, int productId)
    {
        if (!await _userRepo.IsExistAsync(userId))
        {
            throw new NotFoundException("User");
        }
        
        if (!await _productRepo.IsExistAsync(productId))
        {
            throw new NotFoundException("Product");
        }
        
        if (await _userFavoritesRepo.IsExistAsync(userId, productId))
        {
            return;
        }

        _userFavoritesRepo.Add(userId, productId);
        await _uow.SaveChangesAsync();
    }
    
    public async Task RemoveAsync(int userId, int productId)
    {
        if (!await _userFavoritesRepo.IsExistAsync(userId, productId))
        {
            return;
        }
        
        _userFavoritesRepo.Remove(userId, productId);
        await _uow.SaveChangesAsync();
    }
}