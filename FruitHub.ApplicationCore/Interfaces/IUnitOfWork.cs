namespace FruitHub.ApplicationCore.Interfaces;

public interface IUnitOfWork
{
    IGenericRepository<T, TKey> Repository<T, TKey>()
        where T : class, IEntity<TKey>;
    Task<int> SaveChangesAsync();
}