using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces;

public interface IProductRepository : IGenericRepository<Product, int>
{
    
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    
    Task<IEnumerable<Product>> SortByAsync(ProductSortBy sortBy, SortDirection sortDirection = SortDirection.Asc);
}