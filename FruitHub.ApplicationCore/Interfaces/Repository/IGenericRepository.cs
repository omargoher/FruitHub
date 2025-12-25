using System.Linq.Expressions;

namespace FruitHub.ApplicationCore.Interfaces.Repository;

// I Make Interface has generic Id and every entity implement this interface becuse this generic repo work with any entity even if i type is diff
// I remove includes for GetAll and GetById and Find => take every enity make own repo and implement method and include him need
public interface IGenericRepository<T, TKey> where T : class
{
    Task<T?> GetByIdAsync(TKey id);
    
    Task<IEnumerable<T>?> GetAllAsync();

    Task<IEnumerable<T>?> FindAsync(Expression<Func<T, bool>> filter);

    void Add(T entity);
    
    void Update(T entity);
    
    void Remove(T entity);
}