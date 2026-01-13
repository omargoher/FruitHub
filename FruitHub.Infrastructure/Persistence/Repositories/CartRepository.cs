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

    public async Task<CartResponseDto?> GetByUserIdWithCartItemsAsync(int userId)
    {
        return await _context.Carts
            .AsNoTracking()
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
            }).FirstOrDefaultAsync();
    }
    
    public async Task<Cart?> GetByUserIdWithCartItemsAndProductsAsync(int userId)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }
}