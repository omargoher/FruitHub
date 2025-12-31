using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.DTOs.Order;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Repository;

public interface IOrderRepository : IGenericRepository<Order, int>
{
   Task<IReadOnlyList<OrderResponseDto>> GetOrdersForUser(int userId, OrderQuery query);
   Task<IReadOnlyList<OrderResponseDto>> GetAll(OrderQuery query);
   Task<OrderResponseDto?> GetById(int orderId);
}