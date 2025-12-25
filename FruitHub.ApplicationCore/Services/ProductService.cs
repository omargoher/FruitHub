using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IProductRepository _productRepo;
    private readonly IImageService _imageService;

    public ProductService(IUnitOfWork uow, IImageService imageService)
    {
        _uow = uow;
        _productRepo = uow.Products();
        _imageService = imageService;
    }
    
    public async Task<IReadOnlyList<Product>?> GetAllAsync(ProductSortBy sortBy, SortDirection sortDirection = SortDirection.Asc)
    {
        var includeProperties = new string[] {"Category"};
        
        var products = await _productRepo.GetAllAsync(includeProperties);
        
        products = sortBy switch
        {
            ProductSortBy.Price =>
                sortDirection == SortDirection.Asc
                    ? products.OrderBy(p => p.Price)
                    : products.OrderByDescending(p => p.Price),
        };
        return (IReadOnlyList<Product>?)products;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var includeProperties = new string[] {"Category"};
        
        var categorie = await _productRepo.GetByIdAsync(id, includeProperties);
        return categorie;
    }

    public async Task<IReadOnlyList<Product>?> GetByCategoryAsync(int categoryId)
    {
        var categories = await _productRepo.GetByCategoryAsync(categoryId);
        return (IReadOnlyList<Product>?)categories;
    }

    public async Task<IReadOnlyList<Product>?> SearchAsync(string search)
    {
        var includeProperties = new string[] {"Category"};

        var products = await _productRepo.FindAsync(p =>
                p.Name.Contains(search) || p.Description.Contains(search),
            includeProperties);

        return (IReadOnlyList<Product>?)products;
    }

    public async Task CreateAsync(CreateProductDto dto, ImageDto imageDto)
    {
        if (dto == null)
            throw new ArgumentException("Product Data is required");
        
        var imagePath = await _imageService
            .SaveAsync(imageDto.Content, imageDto.FileName, imageDto.ContentType);

        var product = new Product
        {
            Name = dto.Name ,
            Price = dto.Price,
            Calories = dto.Calories,
            Description = dto.Description,
            Organic = dto.Organic,
            ExpirationPeriodByDays = dto.ExpirationPeriodByDays,
            ImagePath = imagePath,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId,
            AdminId = 1 // in this case i has only one admin
        };
        
        _productRepo.Insert(product);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(UpdateProductDto dto, ImageDto? imageDto = null)
    {
        var product = await _productRepo.GetByIdAsync(dto.Id);
        
        if (product == null)
            throw new KeyNotFoundException("Product not found");

        string oldImagePath = product.ImagePath;
        
        if (imageDto != null)
        {
            product.ImagePath = await _imageService.SaveAsync(imageDto.Content, imageDto.FileName, imageDto.ContentType);
        }
        product.Name = dto.Name ?? product.Name;
        product.Price = dto.Price ?? product.Price;
        product.Calories = dto.Calories ?? product.Calories;
        product.Description = dto.Description ?? product.Description;
        product.Organic = dto.Organic ?? product.Organic;
        product.ExpirationPeriodByDays = dto.ExpirationPeriodByDays ?? product.ExpirationPeriodByDays;
        product.Stock = dto.Stock ?? product.Stock;
        product.CategoryId = dto.CategoryId ?? product.CategoryId ;
        
        _productRepo.Update(product);
        await _uow.SaveChangesAsync();

        if (oldImagePath != product.ImagePath)
        {
            await _imageService.DeleteAsync(oldImagePath);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product == null)
            throw new KeyNotFoundException("Product not found");
        
        await _imageService.DeleteAsync(product.ImagePath);
        
        _productRepo.Delete(product);
        await _uow.SaveChangesAsync();
    }
}