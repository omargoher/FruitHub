using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces;

/*
 * TODO CREATE PRODUCT => Done
 * TODO UPDATE PRODUCT => Done
 * TODO DELETE PRODUCT => Done
 * TODO GET ALL PRODUCT => Done
 * TODO GET PRODUCT BY ID => Done
 * TODO SEARCH FOR PRODUCT => Done
 * TODO GET PRODUCT BY THE MOST BUYING => 
 * TODO GET PRODUCTS BY CATEGORY => Done
 * TODO GET NUMBER OF PRODUCTS IN CATEGORY ==> MAY NOT
 * TODO SORT PRODUCTS LIST
 */
public interface IProductService
{
    Task<IReadOnlyList<Product>?> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<IReadOnlyList<Product>?> GetByCategoryAsync(int categoryId);
    Task<IReadOnlyList<Product>?> SearchAsync(string search);
    // Task<IReadOnlyList<Product>?> SortAsync(string searsh);
    Task CreateAsync(CreateProductDto dto, ImageDto imageDto);
    Task UpdateAsync(UpdateProductDto dto, ImageDto? imageDto = null);
    Task DeleteAsync(int id);
}