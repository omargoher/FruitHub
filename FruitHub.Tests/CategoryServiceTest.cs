using FruitHub.ApplicationCore.DTOs.Category;
using FruitHub.ApplicationCore.Exceptions;
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
    
    private CategoryDto ValidCategoryDto()
    {
        return new CategoryDto
        {
            Name = "Fruit",
        };
    }
    
    private CategoryService CreateSut()
    {
        return new CategoryService(
            _uow.Object
        );
     }

     [Fact]
     public async Task GetAllAsync_WhenSuccessful_ShouldReturnListOfCategories()
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
     public async Task GetByIdAsync_WhenCategoryNotFound_ShouldThrowNotFoundException()
     {
         // Arrange
         _uow.Setup(x => x.Category.GetByIdAsync(It.IsAny<int>()))
             .ReturnsAsync((Category?)null);
         
         var sut = CreateSut();
         
         // Act & Assert
         await Assert.ThrowsAsync<NotFoundException>(() => sut.GetByIdAsync(1));
     }

     [Fact]
     public async Task GetByIdAsync_WhenSuccessful_ShouldReturnCategory()
     {
         // Arrange
         var category = new Category
         {
             Name = "Old"
         };
         _uow.Setup(x => x.Category.GetByIdAsync(It.IsAny<int>()))
             .ReturnsAsync(category);
         
         var sut = CreateSut();
         
         // Act
         var result = await sut.GetByIdAsync(1);
         
         // ASSERT
         Assert.Equal(category.Name, result.Name);
     }
     
     [Fact]
     public async Task CreateAsync_WhenNameIsExist_ShouldThrowsConflictException()
     {
         // Arrange
         _uow.Setup(x => 
                 x.Category.IsNameExistAsync(It.IsAny<string>()))
             .ReturnsAsync(false);
         
         var sut = CreateSut();
         
         // Act & Assert
         await Assert.ThrowsAsync<ConflictException>(() => sut.CreateAsync(ValidCategoryDto()));
     }
     
     [Fact]
     public async Task CreateAsync_WhenSuccessful_ShouldCreateCategoty()
     {
         // Arrange
         _uow.Setup(x => 
                 x.Category.IsNameExistAsync(It.IsAny<string>()))
             .ReturnsAsync(true);
         
         _uow.Setup(x => 
                 x.Category.Add(It.IsAny<Category>()));
         
         _uow.Setup(x => 
             x.SaveChangesAsync());
         
         var sut = CreateSut();
         
         // Act
         var dto = ValidCategoryDto();
         var category = await sut.CreateAsync(dto);
         
         // Assert
         Assert.Equal(dto.Name, category.Name);
     }

     [Fact]
     public async Task UpdateAsync_WhenCategoryNotFound_ShouldThrowsNotFoundException()
     {
         // Arrange
         _uow.Setup(x => x.Category.GetByIdAsync(It.IsAny<int>()))
             .ReturnsAsync((Category?)null);
         
         var sut = CreateSut();
         
         // Act & Assert
         var dto = ValidCategoryDto();
         await Assert.ThrowsAsync<NotFoundException>(() => sut.UpdateAsync(1, dto));
     }
     
     [Fact]
     public async Task UpdateAsync_WhenNameIsExist_ShouldThrowsConflictException()
     {
         // Arrange
         _uow.Setup(x => x.Category.GetByIdAsync(It.IsAny<int>()))
             .ReturnsAsync(new Category());
         
         _uow.Setup(x => 
                 x.Category.IsNameExistAsync(It.IsAny<string>()))
             .ReturnsAsync(false);
         
         var sut = CreateSut();
         
         // Act & Assert
         var dto = ValidCategoryDto();
         await Assert.ThrowsAsync<ConflictException>(() => sut.UpdateAsync(1, dto));
     }

     [Fact]
     public async Task UpdateAsync_WhenSuccessful_ShouldUpdateCategoty()
     {
         // Arrange
         var category = new Category
         {
             Name = "Old"
         };
         
         _uow.Setup(x => x.Category.GetByIdAsync(It.IsAny<int>()))
             .ReturnsAsync(category);
         
         _uow.Setup(x => 
                 x.Category.IsNameExistAsync(It.IsAny<string>()))
             .ReturnsAsync(true);
         
         var sut = CreateSut();
         
         // Act
         var dto = ValidCategoryDto();
         await sut.UpdateAsync(1, dto);
         
         // Assert
         Assert.Equal(dto.Name, category.Name);
     }
     
     [Fact]
     public async Task DeleteAsync_WhenCategoryNotFound_ShouldThrowsNotFoundException()
     {
         // Arrange
         _uow.Setup(x => x.Category.GetByIdAsync(It.IsAny<int>()))
             .ReturnsAsync((Category?)null);
         
         var sut = CreateSut();
         
         // Act & Assert
         await Assert.ThrowsAsync<NotFoundException>(() => sut.DeleteAsync(1));
     }
     
     [Fact]
     public async Task DeleteAsync_WhenSuccessful_ShouldDeleteCategory()
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
         _uow.Verify(x => x.Category.Remove(category), Times.Once);
         _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
     }
}