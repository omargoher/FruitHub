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
    
    public async Task<IReadOnlyList<CategoryResponseDto>> GetAllAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        
        return categories.Select(c => new CategoryResponseDto
        {
            Id = c.Id,
            Name = c.Name
        }).ToList();
    }
    
    public async Task<CategoryResponseDto> GetByIdAsync(int categoryId)
    {
        var category = await _categoryRepo.GetByIdAsync(categoryId);

        if (category == null)
        {
            throw new NotFoundException("Category");
        }
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        };
    }
    
    public async Task<CategoryResponseDto> CreateAsync(CategoryDto dto)
    {
        if (await _categoryRepo.IsNameExistAsync(dto.Name))
        {
            throw new ConflictException("Category");
        }

        var category = new Category
        {
            Name = dto.Name
        };
        
        _categoryRepo.Add(category);
        await _uow.SaveChangesAsync();

        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        };
    }
    
    public async Task UpdateAsync(int categoryId, CategoryDto dto)
    {
        if (await _categoryRepo.IsNameExistAsync(dto.Name))
        {
            throw new ConflictException("Category");
        }
        
        var category = await _categoryRepo.GetByIdAsync(categoryId);
        
        if (category == null)
        {
            throw new NotFoundException("Category");
        }
        
        category.Name = dto.Name;
        
        _categoryRepo.Update(category);
        await _uow.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(int categoryId)
    {
        var category = await _categoryRepo.GetByIdAsync(categoryId);
        
        if (category == null)
        {
            throw new NotFoundException("Category");
        }
        
        _categoryRepo.Remove(category);
        await _uow.SaveChangesAsync();
    }
}