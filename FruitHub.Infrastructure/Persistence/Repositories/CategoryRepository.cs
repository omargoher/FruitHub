using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace FruitHub.Infrastructure.Persistence.Repositories;

public class CategoryRepository : GenericRepository<Category, int>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) :base(context)
    {
        
    }
    public async Task<bool> IsNameExistAsync(string categoryName)
    {
        return await _context.Categories
            .AnyAsync(c => c.Name == categoryName);
    }
}