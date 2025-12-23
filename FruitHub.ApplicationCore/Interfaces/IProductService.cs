using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces;

/*
 * TODO CREATE PRODUCT
 * TODO UPDATE PRODUCT
 * TODO DELETE PRODUCT
 * TODO GET ALL PRODUCT 
 * TODO GET PRODUCT BY ID
 * TODO SEARCH FOR PRODUCT
 * TODO GET PRODUCT BY THE MOST BUYING => MAY NOT
 * TODO GET PRODUCTS BY CATEGORY
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
    Task CreateAsync(CreateProductDto dto, string imagePath);
    Task UpdateAsync(UpdateProductDto dto, string? imagePath);
    Task DeleteAsync(int id);
}