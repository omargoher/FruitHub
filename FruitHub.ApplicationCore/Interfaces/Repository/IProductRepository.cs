using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Repository;

public interface IProductRepository : IGenericRepository<Product, int>
{
    Task<IReadOnlyList<ProductResponseDto>> GetProductsAsync(ProductQuery productQuery);
    Task<SingleProductResponseDto?> GetProductByIdWithCategoryAsync(int id);
    Task<IReadOnlyList<ProductResponseDto>> GetByCategoryAsync(int categoryId, ProductQuery productQuery);
    
}