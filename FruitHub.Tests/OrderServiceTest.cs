using FruitHub.ApplicationCore.DTOs.Order;
using FruitHub.ApplicationCore.Enums.Order;
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
    public async Task CreateAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(false);

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.CreateAsync(1, new CreateOrderDto()));
    }

    [Fact]
    public async Task CreateAsync_WhenCartIsEmpty_ShouldThrowInvalidRequestException()
    {
        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(true);
        _cartRepo.Setup(x => x.GetByUserIdWithCartItemsAndProductsAsync(1))
                 .ReturnsAsync(new Cart { Items = new List<CartItem>() });

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidRequestException>(() =>
            sut.CreateAsync(1, new CreateOrderDto()));
    }

    [Fact]
    public async Task CreateAsync_WhenProductStockIsInsufficient_ShouldThrowInvalidRequestException()
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
            sut.CreateAsync(1, new CreateOrderDto()));
    }

    [Fact]
    public async Task CreateAsync_WhenValid_ShouldCreateOrderAndClearCart()
    {
        var product = new Product { Id = 1,Name = "p1", Stock = 10, Price = 20 };
        var cart = new Cart
        {
            Items = new List<CartItem>
            {
                new CartItem { ProductId = 1, Quantity = 2, Product = product }
            }
        };

        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(true);
        _cartRepo.Setup(x => x.GetByUserIdWithCartItemsAndProductsAsync(1))
                 .ReturnsAsync(cart);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var sut = CreateSut();

        await sut.CreateAsync(1, new CreateOrderDto
        {
            CustomerFullName = "Omar",
            CustomerAddress = "Cairo"
        });

        Assert.Empty(cart.Items);
        Assert.Equal(8, product.Stock);
    }
    
    [Fact]
    public async Task UpdateStatusAsync_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _orderRepo.Setup(x => x.GetByIdAsync(1))
                  .ReturnsAsync((Order?)null);

        var sut = CreateSut();

        // ACT & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateStatusAsync(1, new UpdateOrderStatusDto()));
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenValidTransition_ShouldUpdatesOrder()
    {
        // Arrange
        var order = new Order();
        order.UserId = 1;
        order.ChangeStatus(OrderStatus.Shipped);
        
        _orderRepo.Setup(x => x.GetByIdAsync(1))
                  .ReturnsAsync(order);

        var sut = CreateSut();

        // ACT
        await sut.UpdateStatusAsync(1, new UpdateOrderStatusDto {OrderStatus = OrderStatus.Delivered});
        
        // Assert
        Assert.Equal(OrderStatus.Delivered, order.OrderStatus);
    }
    
    [Fact]
    public async Task UpdateStatusAsync_WhenInValidTransition_ShouldThrowDomainException()
    {
        // Arrange
        var order = new Order();
        order.UserId = 1;
        order.ChangeStatus(OrderStatus.Shipped);
        
        _orderRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(order);

        var sut = CreateSut();

        // ACT & Assert
        await Assert.ThrowsAsync<DomainException>(() =>
            sut.UpdateStatusAsync(1, new UpdateOrderStatusDto {OrderStatus = OrderStatus.Pending}));
    }
    
    [Fact]
    public async Task CancelAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepo.Setup(x => x.IsExistAsync(1))
            .ReturnsAsync(false);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.CancelAsync(1, 10));
    }

    [Fact]
    public async Task CancelAsync_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepo.Setup(x => x.IsExistAsync(1))
            .ReturnsAsync(true);

        _orderRepo.Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync((Order?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.CancelAsync(1, 10));
    }

    [Fact]
    public async Task CancelAsync_WhenOrderBelongsToAnotherUser_ShouldThrowForbiddenException()
    {
        // Arrange
        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(true);
        _orderRepo.Setup(x => x.GetByIdAsync(10))
                  .ReturnsAsync(new Order { UserId = 2 });

        var sut = CreateSut();

        // ACT & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            sut.CancelAsync(1, 10));
    }
   
    [Fact]
    public async Task CancelAsync_WhenOrderStatusNotPending_ShouldThrowDomainException()
    {
        // Arrange
        var order = new Order();
        order.UserId = 1;
        order.ChangeStatus(OrderStatus.Shipped);
        
        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(true);
        _orderRepo.Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(order);

        var sut = CreateSut();

        // ACT & Assert
        await Assert.ThrowsAsync<DomainException>(() =>
            sut.CancelAsync(1, 10));
    }
    
    [Fact]
    public async Task CancelAsync_WhenOrderStatusIsPending_ShouldCancelTheOrder()
    {
        // Arrange
        var order = new Order();
        order.UserId = 1;
        
        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(true);
        _orderRepo.Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(order);

        var sut = CreateSut();

        // ACT
        await sut.CancelAsync(1, 10);
        
        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.OrderStatus);
    }
}
