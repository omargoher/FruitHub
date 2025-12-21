using System.Linq.Expressions;
using FruitHub.ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FruitHub.Infrastructure.Persistence.Repositories;

public class GenericRepository<T, TKey> : IGenericRepository<T, TKey> where T : class, IEntity<TKey>
{
    protected readonly ApplicationDbContext _context;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<T?> GetByIdAsync(TKey id, string[]? includeProperties = null)
    {
        IQueryable<T> query = _context.Set<T>();

        if (includeProperties != null)
        {
            query = query.Include(string.Join(".", includeProperties));
        }

        return await query.SingleOrDefaultAsync(t =>
            EqualityComparer<TKey>.Default.Equals(t.Id, id));
    }

    public async Task<IEnumerable<T>> GetAllAsync(string[]? includeProperties = null)
    {
        IQueryable<T> query = _context.Set<T>();

        if (includeProperties != null)
        {
            query = query.Include(string.Join(".", includeProperties));
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>>? filter = null
        , string[]? includeProperties = null)
    {
        IQueryable<T> query = _context.Set<T>();

        if (filter != null)
        {
            query = query.Where(filter);
        }
        if (includeProperties != null)
        {
            query = query.Include(string.Join(".", includeProperties));
        }

        return await query.ToListAsync();
    }

    public void Insert(T entity)
    {
        _context.Set<T>().Add(entity);
    }

    public void Update(T entityToUpdate)
    {
        _context.Set<T>().Update(entityToUpdate);
    }
    
    public void DeleteById(TKey id)
    {
        var entityToDelete = Activator.CreateInstance<T>();
        entityToDelete.Id = id;
        
        // if (entityToDelete == null)
            // throw new KeyNotFoundException($"{typeof(T).Name} not found");
        
        Delete(entityToDelete);
    }

    public void Delete(T entityToDelete)
    {
        _context.Set<T>().Remove(entityToDelete);
    }

    
}