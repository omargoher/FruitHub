using FruitHub.ApplicationCore.DTOs.Category;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync();
    Task CreateAsync(string name);
    Task UpdateAsync(UpdateCategoryDto dto);
    Task DeleteAsync(int id);
}