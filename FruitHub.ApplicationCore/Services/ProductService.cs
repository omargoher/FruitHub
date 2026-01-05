using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.ApplicationCore.Interfaces.Repository;
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
        _productRepo = uow.Product;
        _imageService = imageService;
    }
    
    public async Task<IReadOnlyList<ProductResponseDto>> GetAllAsync(ProductQuery productQuery)
    {
        var products = await _productRepo.GetAllAsync(productQuery);
        return products;
    }

    public async Task<SingleProductResponseDto?> GetByIdAsync(int productId)
    {
        var product = await _productRepo.GetByIdWithCategoryNameAsync(productId);
        return product;
    }

    public async Task<IReadOnlyList<ProductResponseDto>> GetByCategoryAsync(int categoryId, ProductQuery productQuery)
    {
        var products = await _productRepo.GetByCategoryIdAsync(categoryId, productQuery);
        return products;
    }
    
    public async Task CreateAsync(int adminId, CreateProductDto dto, ImageDto imageDto)
    {
        var admin = await _uow.Admin.GetByIdAsync(adminId);
        if (admin == null)
        {
            throw new InvalidRequestException("Admin id not valid");
        }
        
        if (dto == null)
        {
            throw new InvalidRequestException("Product Data is required");
        }

        var category = _uow.Category.GetByIdAsync(dto.CategoryId);
        if (category == null)
        {
            throw new InvalidRequestException("Category id not valid");
        }
        
        if (imageDto == null)
        {
            throw new InvalidRequestException("Image is required");
        }
        
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
            AdminId = adminId
        };
        
        _productRepo.Add(product);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(int productId, UpdateProductDto dto, ImageDto? imageDto = null)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        
        if (product == null)
            throw new NotFoundException("Product not found");

        string oldImagePath = product.ImagePath;
        
        if (imageDto != null)
        {
            product.ImagePath = await _imageService.SaveAsync(imageDto.Content, imageDto.FileName, imageDto.ContentType);
        }

        if (dto.CategoryId.HasValue)
        {
            var category = _uow.Category.GetByIdAsync(dto.CategoryId.Value);
            if (category == null)
            {
                throw new InvalidRequestException("Category id not valid");
            }
        }
        
        product.Name = dto.Name ?? product.Name;
        product.Price = dto.Price ?? product.Price;
        product.Calories = dto.Calories ?? product.Calories;
        product.Description = dto.Description ?? product.Description;
        product.Organic = dto.Organic ?? product.Organic;
        product.ExpirationPeriodByDays = dto.ExpirationPeriodByDays ?? product.ExpirationPeriodByDays;
        product.Stock = dto.Stock ?? product.Stock;
        product.CategoryId = dto.CategoryId ?? product.CategoryId;
        
        _productRepo.Update(product);
        await _uow.SaveChangesAsync();

        if (oldImagePath != product.ImagePath)
        {
            await _imageService.DeleteAsync(oldImagePath);
        }
    }

    public async Task DeleteAsync(int productId)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null)
            throw new NotFoundException("Product not found");
        
        await _imageService.DeleteAsync(product.ImagePath);
        
        _productRepo.Remove(product);
        await _uow.SaveChangesAsync();
    }
}