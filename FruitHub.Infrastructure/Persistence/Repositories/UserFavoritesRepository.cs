using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FruitHub.Infrastructure.Persistence.Repositories;

public class UserFavoritesRepository : IUserFavoritesRepository
{
    protected readonly ApplicationDbContext _context;

    public UserFavoritesRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IReadOnlyList<ProductResponseDto>> GetUserFavoriteListAsync(string identityUserId)
    {
        return await _context.UserFavorites
            .Where(uf => uf.User.UserId == identityUserId)
            .Select(
                uf => new ProductResponseDto
                {
                    Id = uf.ProductId,
                    Name = uf.Product.Name,
                    Price = uf.Product.Price,
                    ImagePath = uf.Product.ImagePath
                })
            .ToListAsync();
    }

    public async Task<bool> CheckIfProductExist(int userId, int productId)
    {
        return await _context.UserFavorites
            .AnyAsync(uf =>
                uf.UserId == userId
                && uf.ProductId == productId);
    }

    public void Add(int userId, int productId)
    {
        _context.UserFavorites.Add(new UserFavorite
        {
            UserId = userId,
            ProductId = productId
        });
    }
    
    public void Remove(int userId, int productId)
    {
        var favoriteProduct = new UserFavorite
        {
            UserId = userId,
            ProductId = productId
        };
        _context.UserFavorites.Remove(favoriteProduct);
    }
}