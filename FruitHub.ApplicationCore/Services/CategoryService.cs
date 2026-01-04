using System.Runtime.InteropServices;
using FruitHub.ApplicationCore.DTOs.Category;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly ICategoryRepository _categoryRepo;

    public CategoryService(IUnitOfWork uow)
    {
        _uow = uow;
        _categoryRepo = uow.Category;
    }
    
    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        
        return categories;
    }
    
    public async Task CreateAsync(CreateCategoryDto dto)
    {
        if (dto == null)
        {
            throw new InvalidRequestException("Category DTO is required");
        }

        _categoryRepo.Add(new Category
        {
            Name = dto.Name
        });
        await _uow.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(int id, UpdateCategoryDto dto)
    {
        if (dto == null)
        {
            throw new InvalidRequestException("Category DTO is required");
        }

        var category = await _categoryRepo.GetByIdAsync(id);
        
        if (category == null)
        {
            throw new NotFoundException("Category not found");
        }
        
        category.Name = dto.Name;
        
        _categoryRepo.Update(category);
        await _uow.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        
        if (category == null)
        {
            throw new NotFoundException("Category not found");
        }
        
        _categoryRepo.Remove(category);
        await _uow.SaveChangesAsync();
    }
}