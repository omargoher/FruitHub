using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.DTOs.Order;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderResponseDto>> GetAllAsync(int userId, OrderQuery query);
    Task<IReadOnlyList<OrderResponseDto>> GetAllAsync(OrderQuery query);
    Task<OrderResponseDto?> GetByIdAsync(int orderId);
    Task<OrderResponseDto?> GetByIdAsync(int userId, int orderId);
    Task<OrderResponseDto> CreateAsync(int userId, CreateOrderDto dto);
    Task UpdateStatusAsync(int orderId, UpdateOrderStatusDto dto);
    Task CancelAsync(int userId, int orderId);
}