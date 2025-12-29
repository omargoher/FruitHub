using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.DTOs.User;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.ApplicationCore.Models;

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

    public async Task<IReadOnlyList<ProductResponseDto>> GetAsync(string identityUserId)
    {
        var products = await _userFavoritesRepo.GetUserFavoriteListAsync(identityUserId);
        
        return products;
    }

    public async Task AddAsync(string identityUserId, int productId)
    {
        var user = await _userRepo.GetByIdentityUserIdAsync(identityUserId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        if (!await _productRepo.CheckIfProductExist(productId))
        {
            throw new KeyNotFoundException("Product not found");
        }
        
        if (await _userFavoritesRepo.CheckIfProductExist(user.Id, productId))
        {
            return;
        }

        _userFavoritesRepo.Add(user.Id, productId);
        await _uow.SaveChangesAsync();
    }
    
    public async Task RemoveAsync(string identityUserId, int productId)
    {
        var user = await _userRepo.GetByIdentityUserIdAsync(identityUserId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        if (!await _productRepo.CheckIfProductExist(productId))
        {
            throw new KeyNotFoundException("Product not found");
        }
        
        if (!await _userFavoritesRepo.CheckIfProductExist(user.Id, productId))
        {
            return;
        }
        
        _userFavoritesRepo.Remove(user.Id, productId);
        await _uow.SaveChangesAsync();
    }
}