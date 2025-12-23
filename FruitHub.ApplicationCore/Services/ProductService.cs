using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IProductRepository _productRepo;

    public ProductService(IUnitOfWork uow)
    {
        _uow = uow;
        _productRepo = uow.Products();
    }
    
    public async Task<IReadOnlyList<Product>?> GetAllAsync()
    {
        var includeProperties = new string[] {"Category"};
        
        var categories = await _productRepo.GetAllAsync(includeProperties);
        return (IReadOnlyList<Product>?)categories;
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

    public async Task CreateAsync(CreateProductDto dto, string imagePath)
    {
        if (dto == null)
            throw new ArgumentException("Product Data is required");
        
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
            AdminId = 1
        };
        
        _productRepo.Insert(product);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(UpdateProductDto dto, string? imagePath = null)
    {
        if (dto == null)
            return;

        var product = await _productRepo.GetByIdAsync(dto.Id);
        
        if (product == null)
            throw new KeyNotFoundException("Product not found");

        product.Id = dto.Id;
        product.Name = dto.Name ?? product.Name;
        product.Price = dto.Price ?? product.Price;
        product.Calories = dto.Calories ?? product.Calories;
        product.Description = dto.Description ?? product.Description;
        product.Organic = dto.Organic ?? product.Organic;
        product.ExpirationPeriodByDays = dto.ExpirationPeriodByDays ?? product.ExpirationPeriodByDays;
        product.ImagePath = imagePath ?? product.ImagePath;
        product.Stock = dto.Stock ?? product.Stock;
        product.CategoryId = dto.CategoryId ?? product.CategoryId ;
        
        _productRepo.Update(product);
        await _uow.SaveChangesAsync();
        
    }

    public async Task DeleteAsync(int id)
    {
        _productRepo.DeleteById(id);
        await _uow.SaveChangesAsync();
    }
}