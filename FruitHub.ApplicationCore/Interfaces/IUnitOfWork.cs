namespace FruitHub.ApplicationCore.Interfaces.Repository;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Category { get; }
    IProductRepository Product { get; }
    Task<int> SaveChangesAsync();
}