using System.Runtime.InteropServices;
using FruitHub.ApplicationCore.DTOs.Category;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Services;
/*
 * TODO CREATE CATEGORY => DONE
 * TODO UPDATE CATEGORY => DONE
 * TODO DELETE CATEGORY => DONE
 * TODO GET ALL CATEGORY  => DONE
 * TODO Refactore Exception => Create Custom Exception to pass code and message
 */
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly ICategoryRepository _categoryRepo;

    public CategoryService(IUnitOfWork uow)
    {
        _uow = uow;
        _categoryRepo = uow.Category;
    }

    /*
     * I use IReadOnlyList becuase
     * 1. is aleady list not query .. it is executed
     * 2. not use List<> becuase not anyone modify it
     */
    public async Task<IReadOnlyList<Category>?> GetAllAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        return (IReadOnlyList<Category>?)categories;
    }
    
    public async Task CreateAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required");
        
        var category = new Category
        {
            Name = name 
        };
        
        _categoryRepo.Add(category);
        await _uow.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(UpdateCategoryDto dto)
    {
        if (dto == null)
            throw new ArgumentException("Category name is required");
        
        var category = await _categoryRepo.GetByIdAsync(dto.Id);
        
        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
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
            throw new KeyNotFoundException("Category not found");
        }
        
        _categoryRepo.Remove(category);
        await _uow.SaveChangesAsync();
    }
}