using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Services;

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
    Task<IReadOnlyList<ProductResponseDto>> GetAllAsync(ProductQuery productQuery);
    
    Task<SingleProductResponseDto?> GetByIdAsync(int id);
    
    Task<IReadOnlyList<ProductResponseDto>> GetByCategoryAsync(int categoryId, ProductQuery productQuery);
    
    Task CreateAsync(CreateProductDto dto, ImageDto imageDto);
    
    Task UpdateAsync(UpdateProductDto dto, ImageDto? imageDto = null);
    
    Task DeleteAsync(int id);
}