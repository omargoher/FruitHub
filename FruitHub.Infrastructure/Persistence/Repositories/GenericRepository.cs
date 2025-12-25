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

    // Add Method return quary to can extend another things like orderBy and delete code include includes in another methods
    public IQueryable<T> Query(string[]? includes)
    {
        IQueryable<T> query = _context.Set<T>();

        if (includes != null)
        {
            query = query.Include(string.Join(".", includes));
        }

        return query.AsNoTracking();
    }
    
    public async Task<T?> GetByIdAsync(TKey id, string[]? includeProperties = null)
    {
        IQueryable<T> query = Query(includeProperties);
        
        // I Use EF.Prpperty => because i use generic repo with generic Key and must every entity use this generic repo include "Id" property  
        return await query.SingleOrDefaultAsync(t =>
            EF.Property<TKey>(t, "Id").Equals(id));
    }

    public async Task<IEnumerable<T>> GetAllAsync(string[]? includeProperties = null)
    {
        IQueryable<T> query = Query(includeProperties);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>>? filter = null
        , string[]? includeProperties = null)
    {
        IQueryable<T> query = Query(includeProperties);

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
        
        if (entityToDelete == null)
            throw new KeyNotFoundException($"{typeof(T).Name} not found");
        
        Delete(entityToDelete);
    }

    public void Delete(T entityToDelete)
    {
        _context.Set<T>().Remove(entityToDelete);
    }

    
}