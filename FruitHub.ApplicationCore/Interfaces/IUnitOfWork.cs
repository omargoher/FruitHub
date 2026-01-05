using FruitHub.ApplicationCore.Interfaces.Repository;

namespace FruitHub.ApplicationCore.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository User { get; }
    ICategoryRepository Category { get; }
    IProductRepository Product { get; }
    IUserFavoritesRepository UserFavorites { get;}
    ICartRepository Cart { get;}
    IOrderRepository Order { get;}
    IAdminRepository Admin { get;}
    Task<int> SaveChangesAsync();
}