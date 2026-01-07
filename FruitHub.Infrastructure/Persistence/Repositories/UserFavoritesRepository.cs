using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace FruitHub.Infrastructure.Persistence.Repositories;

public class UserFavoritesRepository : IUserFavoritesRepository
{
    private readonly ApplicationDbContext _context;

    public UserFavoritesRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IReadOnlyList<ProductResponseDto>> GetByUserIdAsync(int userId)
    {
        return await _context.UserFavorites
            .Where(uf => uf.UserId == userId)
            .Select(
                uf => new ProductResponseDto
                {
                    Id = uf.ProductId,
                    Name = uf.Product.Name,
                    Price = uf.Product.Price,
                    ImageUrl = uf.Product.ImageUrl
                })
            .ToListAsync();
    }

    public async Task<bool> IsExistAsync(int userId, int productId)
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