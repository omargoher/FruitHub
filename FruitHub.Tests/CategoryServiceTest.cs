using FruitHub.ApplicationCore.DTOs.Category;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Services;
using FruitHub.Infrastructure.Services;
using Moq;

namespace FruitHub.Tests;

public class CategoryServiceTest
{
    private readonly Mock<IUnitOfWork> _uow;
    // private readonly ICategoryRepository _categoryRepo;
    
    public CategoryServiceTest()
    {
        _uow = new Mock<IUnitOfWork>();
    }
    
    private CategoryService CreateSut()
    {
        return new CategoryService(
            _uow.Object
        );
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnListOfCategories()
    {
        // Arrange 
        _uow.Setup(x => x.Category.GetAllAsync())
            .ReturnsAsync(new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "C1"
                },
                new Category
                {
                    Id = 2,
                    Name = "C2"
                }
            });
        var sut = CreateSut();
        
        // Act
        var result = await sut.GetAllAsync();
        
        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsEmptyOrNullOrWhiteSpace_ThrowArgumentException()
    {
        // Arrange
        var sut = CreateSut();
        
        // Act
        var result = () => sut.CreateAsync(" ");
        
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(result);
    }
    
    [Fact]
    public async Task CreateAsync_WhenValid_AddCategory()
    {
        // Arrange
        _uow.Setup(x => x.Category.Add(It.IsAny<Category>()));
        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        var sut = CreateSut();
        
        // Act
        await sut.CreateAsync("c1");
        
        // Assert
        _uow.Verify(x => x.Category.Add(It.IsAny<Category>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenDtoNull_ThrowArgumentException()
    {
        // Arrange
        var sut = CreateSut();
        
        // Act
        var result = () => sut.UpdateAsync(null);
        
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(result);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenCategoryNotFound_ThrowKeyNotFoundException()
    {
        // Arrange
        _uow.Setup(x => x.Category.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Category?)null);
        var sut = CreateSut();
        
        // Act
        var result = () => sut.UpdateAsync(new UpdateCategoryDto());
        
        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(result);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenValid_UpdateCategory()
    {
        // Arrange
        var category = new Category
        {
            Id = 1,
            Name = "C1"
        };
        var dto = new UpdateCategoryDto
        {
            Id = 1,
            Name = "c2"
        };
        _uow.Setup(x => x.Category.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(category);
        _uow.Setup(x => x.Category.Update(It.IsAny<Category>()));
        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        
        var sut = CreateSut();
        
        // Act
        await sut.UpdateAsync(dto);
        
        // Assert
        Assert.Equal(dto.Name, category.Name);
        _uow.Verify(x => x.Category.Update(It.IsAny<Category>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task DeleteAsync_WhenCategoryNotFound_ThrowKeyNotFoundException()
    {
        // Arrange
        _uow.Setup(x => x.Category.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Category?)null);
        var sut = CreateSut();
        
        // Act
        var result = () => sut.DeleteAsync(1);
        
        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(result);
    }
    
    [Fact]
    public async Task DeleteAsync_WhenValid_DeleteCategory()
    {
        // Arrange
        var category = new Category
        {
            Id = 1,
            Name = "C1"
        };
        _uow.Setup(x => x.Category.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(category);
        _uow.Setup(x => x.Category.Remove(It.IsAny<Category>()));
        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        
        var sut = CreateSut();
        
        // Act
        await sut.DeleteAsync(1);
        
        // Assert
        _uow.Verify(x => x.Category.Remove(It.IsAny<Category>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}