using FruitHub.ApplicationCore.DTOs.Category;
using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Services;
using FruitHub.Infrastructure.Services;
using Moq;

namespace FruitHub.Tests;

public class ProductServiceTest
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IImageService> _imageService;
    private readonly Mock<IImageValidator> _imageValidator;
    
    public ProductServiceTest()
    {
        _uow = new Mock<IUnitOfWork>();
        _imageService = new Mock<IImageService>();
        _imageValidator = new Mock<IImageValidator>();
    }
    
    private ProductService CreateSut()
    {
        return new ProductService(
            _uow.Object,
            _imageService.Object,
            _imageValidator.Object
        );
    }

    private CreateProductDto ValidCreateProductDto()
    {
        return new CreateProductDto
        {
            Name = "Apple",
            Price = 10,
            Calories = 50,
            Description = "Fresh",
            Organic = true,
            ExpirationPeriodByDays = 7,
            Stock = 100,
            CategoryId = 2
        };
    }

    private ImageDto ValidImageDto()
    {
        return new ImageDto
        {
            Content = Stream.Null,
            Length = 5000,
            ContentType = "image/png",
        };
    }
    
    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldReturnProducts()
    {
        // Arrange
        var query = new ProductQuery();

        var products = new List<ProductResponseDto>
        {
            new() { Id = 1, Name = "Apple" },
            new() { Id = 2, Name = "Banana" }
        };

        _uow.Setup(x => x.Product.GetAllAsync(query))
            .ReturnsAsync(products);

        var sut = CreateSut();

        // Act
        var result = await sut.GetAllAsync(query);

        // Assert
        Assert.Equal(products, result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _uow.Setup(x => x.Product.GetByIdWithCategoryNameAsync(1))
            .ReturnsAsync((SingleProductResponseDto?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.GetByIdAsync(1));
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ShouldReturnProduct()
    {
        // Arrange
        var product = new SingleProductResponseDto
        {
            Id = 1,
            Name = "Apple",
            CategoryName = "Fruits"
        };

        _uow.Setup(x => x.Product.GetByIdWithCategoryNameAsync(1))
            .ReturnsAsync(product);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(1);

        // Assert
        Assert.Equal(product, result);
    }
    
    [Fact]
    public async Task GetByCategoryAsync_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _uow.Setup(x => x.Category.IsExistAsync(1))
            .ReturnsAsync(false);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.GetByCategoryAsync(1, new ProductQuery()));
    }
    
    [Fact]
    public async Task GetByCategoryAsync_WhenCategoryExists_ShouldReturnProducts()
    {
        // Arrange
        var query = new ProductQuery();
        var categoryId = 2;

        var products = new List<ProductResponseDto>
        {
            new() { Id = 1, Name = "Orange" }
        };

        _uow.Setup(x => x.Category.IsExistAsync(categoryId))
            .ReturnsAsync(true);

        _uow.Setup(x =>
                x.Product.GetByCategoryIdAsync(categoryId, query))
            .ReturnsAsync(products);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByCategoryAsync(categoryId, query);

        // Assert
        Assert.Equal(products, result);
    }

    [Fact]
    public async Task CreateAsync_WhenAdminNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var dto = new CreateProductDto { CategoryId = 1 };
        var image = new ImageDto();

        _uow.Setup(x => x.Admin.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Admin?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.CreateAsync(1, dto, image));
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var adminId = 1;

        var dto = new CreateProductDto
        {
            CategoryId = 99
        };

        var image = new ImageDto();

        _uow.Setup(x => x.Admin.GetByIdAsync(adminId))
            .ReturnsAsync(new Admin { Id = adminId });

        _uow.Setup(x => x.Category.GetByIdAsync(dto.CategoryId))
            .ReturnsAsync((Category?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.CreateAsync(adminId, dto, image));
    }

    [Fact]
    public async Task CreateAsync_WhenImageInvalid_ShouldThrowException()
    {
        // Arrange
        var adminId = 1;

        var dto = new CreateProductDto
        {
            CategoryId = 1
        };

        var image = new ImageDto();

        _uow.Setup(x => x.Admin.GetByIdAsync(adminId))
            .ReturnsAsync(new Admin { Id = adminId });

        _uow.Setup(x => x.Category.GetByIdAsync(dto.CategoryId))
            .ReturnsAsync(new Category { Id = dto.CategoryId });

        _imageValidator
            .Setup(x => x.Validate(image))
            .Throws(new InvalidRequestException("Invalid image"));

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidRequestException>(() =>
            sut.CreateAsync(adminId, dto, image));
    }

    [Fact]
    public async Task CreateAsync_WhenImageSaveFails_ShouldThrowException()
    {
        // Arrange
        var adminId = 1;

        var dto = new CreateProductDto
        {
            CategoryId = 1
        };

        var image = new ImageDto();

        _uow.Setup(x => x.Admin.GetByIdAsync(adminId))
            .ReturnsAsync(new Admin { Id = adminId });

        _uow.Setup(x => x.Category.GetByIdAsync(dto.CategoryId))
            .ReturnsAsync(new Category { Id = dto.CategoryId });

        _imageValidator.Setup(x => x.Validate(image));

        _imageService
            .Setup(x => x.SaveAsync(image, "products"))
            .ThrowsAsync(new Exception("Storage error"));

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            sut.CreateAsync(adminId, dto, image));
        
    }

    [Fact]
    public async Task CreateAsync_WhenImageSaveFails_ShouldNotAddProduct()
    {
        // Arrange
        var adminId = 1;

        var dto = new CreateProductDto
        {
            CategoryId = 1
        };

        var image = new ImageDto();

        _uow.Setup(x => x.Admin.GetByIdAsync(adminId))
            .ReturnsAsync(new Admin { Id = adminId });

        _uow.Setup(x => x.Category.GetByIdAsync(dto.CategoryId))
            .ReturnsAsync(new Category { Id = dto.CategoryId });

        _imageValidator.Setup(x => x.Validate(image));

        _imageService
            .Setup(x => x.SaveAsync(image, "products"))
            .ThrowsAsync(new Exception("Storage error"));

        var sut = CreateSut();

        // Act 
        try
        {
            await sut.CreateAsync(adminId, dto, image);
        }
        catch
        {
            
        }   
        
        // Assert
        _uow.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
    [Fact]
    public async Task CreateAsync_WhenValid_ShouldCreateProduct()
    {
        // Arrange
        var adminId = 1;
        var dto = ValidCreateProductDto();
        var image = ValidImageDto();

        var admin = new Admin { Id = adminId };
        var category = new Category { Id = dto.CategoryId };

        _uow.Setup(x => x.Admin.GetByIdAsync(adminId))
            .ReturnsAsync(admin);

        _uow.Setup(x => x.Category.GetByIdAsync(dto.CategoryId))
            .ReturnsAsync(category);

        _imageValidator
            .Setup(x => x.Validate(image));

        _imageService
            .Setup(x => x.SaveAsync(image, "products"))
            .ReturnsAsync("/images/products/apple.png");

        _uow.Setup(x => x.Product.Add(It.IsAny<Product>()));

        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        await sut.CreateAsync(adminId, dto, image);

        // Assert
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenDtoAndImageAreNull_ShouldReturnWithoutCallingAnything()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await sut.UpdateAsync(1, null!, null);

        // Assert
        _uow.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _uow.Setup(x => x.Product.GetByIdAsync(1))
            .ReturnsAsync((Product?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateAsync(1, new UpdateProductDto(), null));
    }

    [Fact]
    public async Task UpdateAsync_WhenDtoProvided_ShouldUpdateProductFields()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Old",
            Price = 10,
            Stock = 5,
            ImageUrl = "old.png",
            CategoryId = 1
        };

        var dto = new UpdateProductDto
        {
            Name = "New",
            Price = 20,
            Stock = 10
        };

        _uow.Setup(x => x.Product.GetByIdAsync(1))
            .ReturnsAsync(product);

        _uow.Setup(x => x.Product.Update(product));

        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        await sut.UpdateAsync(1, dto);

        // Assert
        Assert.Equal(dto.Name, product.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenImageProvided_ShouldReplaceAndDeleteOldImage()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            ImageUrl = "old.png",
            CategoryId = 1
        };

        var image = new ImageDto();

        _uow.Setup(x => x.Product.GetByIdAsync(1))
            .ReturnsAsync(product);

        _imageValidator.Setup(x => x.Validate(image));

        _imageService
            .Setup(x => x.SaveAsync(image, "products"))
            .ReturnsAsync("new.png");

        _imageService
            .Setup(x => x.DeleteAsync("old.png"))
            .Returns(Task.CompletedTask);

        _uow.Setup(x => x.Product.Update(product));

        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        await sut.UpdateAsync(1, new UpdateProductDto(), image);

        // Assert
        _imageService.Verify(x => x.SaveAsync(image, "products"), Times.Once);
        _imageService.Verify(x => x.DeleteAsync("old.png"), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            CategoryId = 1
        };

        var dto = new UpdateProductDto
        {
            CategoryId = 99
        };

        _uow.Setup(x => x.Product.GetByIdAsync(1))
            .ReturnsAsync(product);

        _uow.Setup(x => x.Category.GetByIdAsync(99))
            .ReturnsAsync((Category?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateAsync(1, dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryExists_ShouldUpdateCategory()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            CategoryId = 1
        };

        var dto = new UpdateProductDto
        {
            CategoryId = 2
        };

        _uow.Setup(x => x.Product.GetByIdAsync(1))
            .ReturnsAsync(product);

        _uow.Setup(x => x.Category.GetByIdAsync(2))
            .ReturnsAsync(new Category { Id = 2 });

        _uow.Setup(x => x.Product.Update(product));

        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        await sut.UpdateAsync(1, dto);

        // Assert
        Assert.Equal(2, product.CategoryId);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductExists_ShouldDeleteImageAndProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            ImageUrl = "products/old.png"
        };

        _uow.Setup(x => x.Product.GetByIdAsync(1))
            .ReturnsAsync(product);

        _imageService
            .Setup(x => x.DeleteAsync(product.ImageUrl))
            .Returns(Task.CompletedTask);

        _uow.Setup(x => x.Product.Remove(product));

        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        await sut.DeleteAsync(1);

        // Assert
        _imageService.Verify(x => x.DeleteAsync(product.ImageUrl), Times.Once);
        _uow.Verify(x => x.Product.Remove(product), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _uow.Setup(x => x.Product.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Product?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.DeleteAsync(1));
    }

    [Fact]
    public async Task DeleteAsync_WhenImageDeleteFails_ShouldNotRemoveProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            ImageUrl = "products/old.png"
        };

        _uow.Setup(x => x.Product.GetByIdAsync(1))
            .ReturnsAsync(product);

        _imageService
            .Setup(x => x.DeleteAsync(product.ImageUrl))
            .ThrowsAsync(new Exception("Storage error"));

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            sut.DeleteAsync(1));
    }

}