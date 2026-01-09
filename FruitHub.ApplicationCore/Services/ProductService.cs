using FruitHub.ApplicationCore.DTOs.Product;
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
    private readonly IImageValidator _imageValidator;

    public ProductService(IUnitOfWork uow, IImageService imageService, IImageValidator imageValidator)
    {
        _uow = uow;
        _productRepo = uow.Product;
        _imageService = imageService;
        _imageValidator = imageValidator;
    }
    
    public async Task<IReadOnlyList<ProductResponseDto>> GetAllAsync(ProductQuery productQuery)
    {
        var products = await _productRepo.GetAllAsync(productQuery);
        return products;
    }

    public async Task<SingleProductResponseDto?> GetByIdAsync(int productId)
    {
        var product = await _productRepo.GetByIdWithCategoryNameAsync(productId);

        if (product == null)
        {
            throw new NotFoundException("Product");
        }
        
        return product;
    }

    public async Task<IReadOnlyList<ProductResponseDto>> GetByCategoryAsync(int categoryId, ProductQuery productQuery)
    {
        if (!await _uow.Category.IsExistAsync(categoryId))
        {
            throw new NotFoundException("Category");
        }
        
        var products = await _productRepo.GetByCategoryIdAsync(categoryId, productQuery);
        return products;
    }
    
    public async Task<SingleProductResponseDto> CreateAsync(int adminId, CreateProductDto dto, ImageDto image)
    {
        var admin = await _uow.Admin.GetByIdAsync(adminId);
        if (admin == null)
        {
            throw new NotFoundException("Admin");
        }
        
        var category = await _uow.Category.GetByIdAsync(dto.CategoryId);
        if (category == null)
        {
            throw new NotFoundException("Category");
        }

        _imageValidator.Validate(image);
        
        var imageUrl = await _imageService
            .SaveAsync(image, "products");

        var product = new Product
        {
            Name = dto.Name ,
            Price = dto.Price,
            Calories = dto.Calories,
            Description = dto.Description,
            Organic = dto.Organic,
            ExpirationPeriodByDays = dto.ExpirationPeriodByDays,
            Stock = dto.Stock,
            ImageUrl = imageUrl,
            CategoryId = dto.CategoryId,
            AdminId = adminId
        };
        
        _productRepo.Add(product);
        await _uow.SaveChangesAsync();

        var response = new SingleProductResponseDto
        {
            Id = product.Id,
            Name = dto.Name ,
            Price = dto.Price,
            Calories = dto.Calories,
            Description = dto.Description,
            Organic = dto.Organic,
            ExpirationPeriodByDays = dto.ExpirationPeriodByDays,
            Stock = dto.Stock,
            ImageUrl = imageUrl,
            CategoryId = dto.CategoryId,
            CategoryName = category.Name,
        };
        return response;
    }

    public async Task UpdateAsync(int productId, UpdateProductDto dto, ImageDto? image = null)
    {
        if (dto == null && image == null)
        {
            return;
        }
        var product = await _productRepo.GetByIdAsync(productId);
        
        if (product == null)
            throw new NotFoundException("Product");

        string oldImage = product.ImageUrl;
        
        if (image != null)
        {
            _imageValidator.Validate(image);

           product.ImageUrl = await _imageService
                .SaveAsync(image, "products");
        }

        if (dto.CategoryId.HasValue)
        {
            var category = await _uow.Category.GetByIdAsync(dto.CategoryId.Value);
            if (category == null)
            {
                throw new NotFoundException("Category");
            }

            product.CategoryId = dto.CategoryId.Value;
        }
        
        product.Name = dto.Name ?? product.Name;
        product.Price = dto.Price ?? product.Price;
        product.Calories = dto.Calories ?? product.Calories;
        product.Description = dto.Description ?? product.Description;
        product.Organic = dto.Organic ?? product.Organic;
        product.ExpirationPeriodByDays = dto.ExpirationPeriodByDays ?? product.ExpirationPeriodByDays;
        product.Stock = dto.Stock ?? product.Stock;
        
        _productRepo.Update(product);

        if (oldImage != product.ImageUrl)
        {
            await _imageService.DeleteAsync(oldImage);
        }
       
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int productId)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null)
            throw new NotFoundException("Product");
        
        await _imageService.DeleteAsync(product.ImageUrl);
        
        _productRepo.Remove(product);
        await _uow.SaveChangesAsync();
    }
}