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
    
}