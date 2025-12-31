using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Models;
using Microsoft.EntityFrameworkCore;

namespace FruitHub.Infrastructure.Persistence.Repositories;

public class CartRepository : GenericRepository<Cart, int>, ICartRepository
{
    public CartRepository(ApplicationDbContext context) :base(context)
    {
    }

    public async Task<IReadOnlyList<CartResponseDto>> GetWithCartItemsAsync(int userId)
    {
        return await _context.Carts
            .Where(c => c.UserId == userId)
            .Select(c => new CartResponseDto
            {
                Items = c.Items.Select(ci => new CartItemResponseDto
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price
                }).ToList(),
                TotalPrice = c.Items.Sum(ci => ci.Quantity * ci.Product.Price)
            }).ToListAsync();
    }
    
    public async Task<Cart?> GetWithCartItemsAndProductsAsync(int userId)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }
    public async Task<CartItem?> GetItemAsync(int userId, int productId)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(ci =>
                ci.Cart.User.Id == userId &&
                ci.ProductId == productId);
    }
    
    public async Task<bool> CheckIfProductExist(int userId, int productId)
    {
        return await _context.Carts
            .AnyAsync(c =>
                c.UserId == userId
                && c.Items.Any(ci => ci.ProductId == productId));
    }
}