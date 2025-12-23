using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces;

public interface IProductRepository : IGenericRepository<Product, int>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
}