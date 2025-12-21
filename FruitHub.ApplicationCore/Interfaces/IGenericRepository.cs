using System.Linq.Expressions;

namespace FruitHub.ApplicationCore.Interfaces;

public interface IGenericRepository<T, TKey> where T : class
{
    Task<T?> GetByIdAsync(TKey id, string[]? includeProperties = null);
    
    Task<IEnumerable<T>?> GetAllAsync(string[]? includeProperties = null);

    Task<IEnumerable<T>?> FindAsync(Expression<Func<T, bool>>? filter = null
        , string[]? includeProperties = null);

    void Insert(T entity);
    
    void Update(T entityToUpdate);
    
    void DeleteById(TKey id);
    
    void Delete(T entityToDelete);
    

}