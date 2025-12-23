using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Models;
using FruitHub.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FruitHub.Infrastructure.Persistence.Repositories;

public class ProductRepository : GenericRepository<Product, int>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) :base(context)
    {
    }
    
    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        var products = _context.Set<Product>()
            .Include(p => p.Category)
            .Where(p => p.Category.Id == categoryId);

        return await products.ToListAsync();
    }
}