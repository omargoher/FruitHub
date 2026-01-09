using FruitHub.ApplicationCore.DTOs.Order;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Services;
using Moq;
using Xunit;

namespace FruitHub.Tests;

public class OrderServiceTest
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IOrderRepository> _orderRepo;
    private readonly Mock<ICartRepository> _cartRepo;

    public OrderServiceTest()
    {
        _uow = new Mock<IUnitOfWork>();
        _userRepo = new Mock<IUserRepository>();
        _orderRepo = new Mock<IOrderRepository>();
        _cartRepo = new Mock<ICartRepository>();

        _uow.Setup(x => x.User).Returns(_userRepo.Object);
        _uow.Setup(x => x.Order).Returns(_orderRepo.Object);
        _uow.Setup(x => x.Cart).Returns(_cartRepo.Object);
    }

    private OrderService CreateSut() => new OrderService(_uow.Object);

    
    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldReturnOrders()
    {
        var orders = new List<OrderResponseDto>
        {
            new OrderResponseDto
            {
                OrderId = 1,
            }
        };

        _orderRepo.Setup(x => x.GetAllWithOrderItemsAsync(It.IsAny<OrderQuery>()))
                  .ReturnsAsync(orders);

        var sut = CreateSut();

        var result = await sut.GetAllAsync(new OrderQuery());

        Assert.Equal(orders, result);
    }

    
    [Fact]
    public async Task GetAllForUserAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(false);

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.GetAllForUserAsync(1, new OrderQuery()));
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenUserExists_ShouldReturnOrders()
    {
        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(true);

        var orders = new List<OrderResponseDto>
        {
            new OrderResponseDto
            {
                OrderId = 1,
            }
        };

        _orderRepo.Setup(x =>
                x.GetByUserIdWithOrderItemsAsync(1, It.IsAny<OrderQuery>()))
            .ReturnsAsync(orders);

        var sut = CreateSut();

        var result = await sut.GetAllForUserAsync(1, new OrderQuery());

        Assert.Equal(orders, result);
    }

   // Admin
    [Fact]
    public async Task GetByIdAsync_WhenOrderExists_ShouldReturnOrder()
    {
        var order = new OrderResponseDto { OrderId = 1 };

        _orderRepo.Setup(x => x.GetByIdWithOrderItemsAsync(1))
                  .ReturnsAsync(order);

        var sut = CreateSut();

        var result = await sut.GetByIdAsync(1);

        Assert.Equal(order, result);
    }
    
    // User
    [Fact]
    public async Task GetByIdAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(false);

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.GetByIdAsync(1, 10));
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderBelongsToAnotherUser_ShouldThrowForbiddenException()
    {
        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(true);

        _orderRepo.Setup(x => x.GetByIdWithOrderItemsAsync(10))
                  .ReturnsAsync(new OrderResponseDto { OrderId = 10, UserId = 2 });

        var sut = CreateSut();

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            sut.GetByIdAsync(1, 10));
    }

    
    [Fact]
    public async Task CheckoutAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(false);

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.CheckoutAsync(1, new CheckoutDto()));
    }

    [Fact]
    public async Task CheckoutAsync_WhenCartIsEmpty_ShouldThrowInvalidRequestException()
    {
        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(true);
        _cartRepo.Setup(x => x.GetByUserIdWithCartItemsAndProductsAsync(1))
                 .ReturnsAsync(new Cart { Items = new List<CartItem>() });

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidRequestException>(() =>
            sut.CheckoutAsync(1, new CheckoutDto()));
    }

    [Fact]
    public async Task CheckoutAsync_WhenProductStockIsInsufficient_ShouldThrowInvalidRequestException()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Apple",
            Stock = 2,
            Price = 10
        };

        var cart = new Cart
        {
            Items = new List<CartItem>
            {
                new()
                {
                    ProductId = 1,
                    Quantity = 3,
                    Product = product
                }
            }
        };

        _userRepo.Setup(x => x.IsExistAsync(1))
            .ReturnsAsync(true);

        _cartRepo.Setup(x => x.GetByUserIdWithCartItemsAndProductsAsync(1))
            .ReturnsAsync(cart);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidRequestException>(() =>
            sut.CheckoutAsync(1, new CheckoutDto()));
    }

    [Fact]
    public async Task CheckoutAsync_WhenValid_ShouldCreateOrderAndClearCart()
    {
        var product = new Product { Id = 1, Stock = 10, Price = 20 };
        var cart = new Cart
        {
            Items = new List<CartItem>
            {
                new() { ProductId = 1, Quantity = 2, Product = product }
            }
        };

        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(true);
        _cartRepo.Setup(x => x.GetByUserIdWithCartItemsAndProductsAsync(1))
                 .ReturnsAsync(cart);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var sut = CreateSut();

        await sut.CheckoutAsync(1, new CheckoutDto
        {
            CustomerFullName = "Omar",
            CustomerAddress = "Cairo"
        });

        Assert.Empty(cart.Items);
        Assert.Equal(8, product.Stock);
    }
    
    [Fact]
    public async Task ChangeOrderStatusAsync_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        _orderRepo.Setup(x => x.GetByIdAsync(1))
                  .ReturnsAsync((Order?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.ChangeOrderStatusAsync(1, new ChangeOrderStatusDto()));
    }

    [Fact]
    public async Task ChangeOrderStatusAsync_WhenShippedAndNotPayed_ShouldThrowInvalidRequestException()
    {
        _orderRepo.Setup(x => x.GetByIdAsync(1))
                  .ReturnsAsync(new Order());

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidRequestException>(() =>
            sut.ChangeOrderStatusAsync(1,
                new ChangeOrderStatusDto { IsShipped = true, IsPayed = false }));
    }

    [Fact]
    public async Task ChangeOrderStatusAsync_WhenShippedTrue_ShouldAlsoSetPayedTrue()
    {
        // Arrange
        var order = new Order
        {
            IsShipped = false,
            IsPayed = false
        };

        _orderRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(order);

        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var dto = new ChangeOrderStatusDto
        {
            IsShipped = true
        };

        var sut = CreateSut();

        // Act
        await sut.ChangeOrderStatusAsync(1, dto);

        // Assert
        Assert.True(order.IsShipped);
        Assert.True(order.IsPayed);
    }

    [Fact]
    public async Task ChangeOrderStatusAsync_WhenPayedFalse_ShouldSetShippedFalse()
    {
        // Arrange
        var order = new Order
        {
            IsPayed = true,
            IsShipped = true
        };

        _orderRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(order);

        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var dto = new ChangeOrderStatusDto
        {
            IsPayed = false
        };

        var sut = CreateSut();

        // Act
        await sut.ChangeOrderStatusAsync(1, dto);

        // Assert
        Assert.False(order.IsPayed);
        Assert.False(order.IsShipped);
    }

    [Fact]
    public async Task ChangeOrderStatusAsync_WhenPayedTrue_ShouldNotForceShippedTrue()
    {
        // Arrange
        var order = new Order
        {
            IsPayed = false,
            IsShipped = false
        };

        _orderRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(order);

        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var dto = new ChangeOrderStatusDto
        {
            IsPayed = true
        };

        var sut = CreateSut();

        // Act
        await sut.ChangeOrderStatusAsync(1, dto);

        // Assert
        Assert.True(order.IsPayed);
        Assert.False(order.IsShipped);
    }

    
    // admin
    [Fact]
    public async Task CancelOrderAsync_WhenOrderNotExists_ShouldThrowNotFoundException()
    {
        _orderRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Order?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.CancelOrderAsync(1));
    }
    
    [Fact]
    public async Task CancelOrderAsync_WhenOrderExists_ShouldCancelOrder()
    {
        var order = new Order { IsPayed = true, IsShipped = true };

        _orderRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var sut = CreateSut();

        await sut.CancelOrderAsync(1);

        Assert.True(order.IsCanceled);
        Assert.False(order.IsPayed);
        Assert.False(order.IsShipped);
    }

    // user
    [Fact]
    public async Task CancelOrderAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepo.Setup(x => x.IsExistAsync(1))
            .ReturnsAsync(false);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.CancelOrderAsync(1, 10));
    }

    [Fact]
    public async Task CancelOrderAsync_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepo.Setup(x => x.IsExistAsync(1))
            .ReturnsAsync(true);

        _orderRepo.Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync((Order?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.CancelOrderAsync(1, 10));
    }

    [Fact]
    public async Task CancelOrderAsync_WhenOrderBelongsToAnotherUser_ShouldThrowForbiddenException()
    {
        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(true);
        _orderRepo.Setup(x => x.GetByIdAsync(10))
                  .ReturnsAsync(new Order { UserId = 2 });

        var sut = CreateSut();

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            sut.CancelOrderAsync(1, 10));
    }
    
    [Fact]
    public async Task CancelOrderAsync_WhenValid_ShouldCancelOrderAndResetState()
    {
        // Arrange
        var order = new Order
        {
            Id = 10,
            UserId = 1,
            IsCanceled = false,
            IsShipped = false,
            IsPayed = false
        };

        _userRepo.Setup(x => x.IsExistAsync(1))
            .ReturnsAsync(true);

        _orderRepo.Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(order);

        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        await sut.CancelOrderAsync(1, 10);

        // Assert
        Assert.True(order.IsCanceled);
        Assert.False(order.IsShipped);
        Assert.False(order.IsPayed);
    }

}
