using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.Infrastructure.Interfaces;
using FruitHub.Infrastructure.Persistence.Repositories;

namespace FruitHub.Infrastructure.Persistence.UnitOfWork;

// i don't use the Dictionary of repos i just add all repos as a prop in unit of work and initialize it in constructor
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        User = new UserRepository(_context);
        Category = new CategoryRepository(_context);
        Product = new ProductRepository(_context);
    }
    public IUserRepository User { get; private set; }
    public ICategoryRepository Category { get; private set; }
    public IProductRepository Product { get; private set; }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }

}