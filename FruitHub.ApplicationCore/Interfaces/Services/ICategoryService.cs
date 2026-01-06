using FruitHub.ApplicationCore.DTOs.Category;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponseDto>> GetAllAsync();
    Task<CategoryResponseDto> GetByIdAsync(int categoryId);
    Task<CategoryResponseDto> CreateAsync(CategoryDto dto);
    Task UpdateAsync(int categoryId, CategoryDto dto);
    Task DeleteAsync(int categoryId);
}