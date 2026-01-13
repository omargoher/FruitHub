using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Repository;

public interface IProductRepository : IGenericRepository<Product, int>
{
    Task<IReadOnlyList<ProductResponseDto>> GetAllAsync(ProductQuery productQuery);
    Task<SingleProductResponseDto?> GetByIdWithCategoryNameAsync(int productId);
    Task<IReadOnlyList<ProductResponseDto>> GetByCategoryIdAsync(int categoryId, ProductQuery productQuery);
}