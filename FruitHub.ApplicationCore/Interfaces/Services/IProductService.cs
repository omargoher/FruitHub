using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductResponseDto>> GetAllAsync(ProductQuery productQuery);
    
    Task<SingleProductResponseDto?> GetByIdAsync(int id);
    
    Task<IReadOnlyList<ProductResponseDto>> GetByCategoryAsync(int categoryId, ProductQuery productQuery);
    
    Task<SingleProductResponseDto> CreateAsync(int adminId, CreateProductDto dto, ImageDto imageDto);
    
    Task UpdateAsync(int id, UpdateProductDto dto, ImageDto? imageDto = null);
    
    Task DeleteAsync(int id);
}