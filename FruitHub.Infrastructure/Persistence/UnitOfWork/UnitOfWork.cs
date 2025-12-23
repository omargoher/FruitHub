using FruitHub.ApplicationCore.Interfaces;
using FruitHub.Infrastructure.Interfaces;
using FruitHub.Infrastructure.Persistence.Repositories;

namespace FruitHub.Infrastructure.Persistence.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    private IProductRepository? _productRepository;
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IProductRepository Products()
    {
        var type = typeof(IProductRepository);
        if (!_repositories.TryGetValue(type, out var repo))
        {
            repo = new ProductRepository(_context);
            _repositories[type] = repo;
        }
        return (IProductRepository)repo;
    }

    public IGenericRepository<T, TKey> Repository<T, TKey>()
        where T : class, IEntity<TKey>
    {
        var type = typeof(T);

        if (!_repositories.TryGetValue(type, out var repo))
        {
            repo = new GenericRepository<T, TKey>(_context);
            _repositories[type] = repo;
        }

        return (IGenericRepository<T, TKey>)repo;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}