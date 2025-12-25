namespace FruitHub.ApplicationCore.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Product { get; }
    Task<int> SaveChangesAsync();
}