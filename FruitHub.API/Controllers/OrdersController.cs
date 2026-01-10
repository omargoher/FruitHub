using System.Security.Claims;
using FruitHub.API.Extensions;
using FruitHub.API.Responses;
using FruitHub.ApplicationCore.DTOs.Order;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

/// <summary>
/// Manages order operations such as listing, checkout, status updates, and cancellation.
/// </summary>
/// <remarks>
/// Access to this controller requires authentication.
///
/// **Role Behavior:**
/// - **Admin**: Can access and manage all orders.
/// - **User**: Can only access and manage their own orders.
/// </remarks>
/// <response code="401">User is not authenticated.</response>
[ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status401Unauthorized)]
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    
    /// <summary>
    /// Retrieves a list of orders.
    /// </summary>
    /// <remarks>
    /// **Role behavior:**
    /// - **Admin**: Retrieves all orders in the system.
    /// - **User**: Retrieves only the authenticated user's orders.
    ///
    /// Supports filtering, sorting, and pagination via query parameters.
    /// </remarks>
    /// <param name="query">Order filtering and pagination parameters.</param>
    /// <returns>A list of orders.</returns>
    /// <response code="200">Orders retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] OrderQuery query)
    {
        IReadOnlyList<OrderResponseDto> orders;
        
        if (IsAdmin())
        {
            orders = await _orderService.GetAllAsync(query);  
        }
        else
        {
            var userId = ClaimsPrincipalExtensions.GetUserId(User);
            orders = await _orderService.GetAllForUserAsync(userId, query);  
        }
        return Ok(orders);
    }
    
    /// <summary>
    /// Retrieves an order by its identifier.
    /// </summary>
    /// <remarks>
    /// **Role behavior:**
    /// - **Admin**: Can retrieve any order.
    /// - **User**: Can only retrieve their own orders.
    ///
    /// Accessing another user's order will result in a forbidden error.
    /// </remarks>
    /// <param name="orderId">Order identifier.</param>
    /// <returns>Order details.</returns>
    /// <response code="200">Order retrieved successfully.</response>
    /// <response code="403">Access to the order is forbidden.</response>
    /// <response code="404">Order not found.</response>
    [HttpGet("{orderId:int}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int orderId)
    {
        OrderResponseDto? order;

        if (IsAdmin())
        {
            order = await _orderService.GetByIdAsync(orderId);
        }
        else
        {
            var userId = ClaimsPrincipalExtensions.GetUserId(User);
            order = await _orderService.GetByIdAsync(userId, orderId);  
        }

        return Ok(order);
    }
    
    /// <summary>
    /// Creates a new order from the user's cart.
    /// </summary>
    /// <remarks>
    /// **Authorization:**
    /// - Requires **User** role.
    ///
    /// **Business Rules:**
    /// - Cart must not be empty.
    /// - Product stock is validated.
    /// - Payment is collected on delivery.
    /// </remarks>
    /// <param name="dto">Checkout information.</param>
    /// <response code="204">Order created successfully.</response>
    /// <response code="400">Invalid request or cart is empty.</response>
    /// <response code="403">User is not authorized (User only)</response>
    [Authorize(Roles = "User")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CheckoutAsync(CheckoutDto dto)
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);
        await _orderService.CheckoutAsync(userId, dto);
        return NoContent();
    }
    
    /// <summary>
    /// Updates the shipping or payment status of an order.
    /// </summary>
    /// <remarks>
    /// **Authorization:**
    /// - Requires **Admin** role.
    ///
    /// **Business Rules:**
    /// - An order cannot be shipped unless it is paid.
    /// - Marking an order as shipped automatically marks it as paid.
    /// </remarks>
    /// <param name="orderId">Order identifier.</param>
    /// <param name="dto">Order status update data.</param>
    /// <response code="204">Order status updated successfully.</response>
    /// <response code="400">Invalid status transition.</response>
    /// <response code="403">User is not authorized (Admin only)</response>
    /// <response code="404">Order not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{orderId:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeOrderStatusAsync(int orderId, ChangeOrderStatusDto dto)
    {
        await _orderService.ChangeOrderStatusAsync(orderId, dto);
        return NoContent();
    }
    
    /// <summary>
    /// Cancels an order.
    /// </summary>
    /// <remarks>
    /// **Role behavior:**
    /// - **Admin**: Can cancel any order.
    /// - **User**: Can only cancel their own orders.
    ///
    /// Canceling an order will:
    /// - Mark it as canceled
    /// - Can not cancel order already shipped
    /// </remarks>
    /// <param name="orderId">Order identifier.</param>
    /// <response code="204">Order canceled successfully.</response>
    /// <response code="403">Access to the order is forbidden.</response>
    /// <response code="400">Order already shipped.</response> 
    /// <response code="404">Order not found.</response>
    [HttpDelete("{orderId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)] // conflict ????
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrderAsync(int orderId)
    {
        if (IsAdmin())
        {
            await _orderService.CancelOrderAsync(orderId);
        }
        else
        {
            var userId = ClaimsPrincipalExtensions.GetUserId(User);
            await _orderService.CancelOrderAsync(userId, orderId);  
        }
        return NoContent();
    }

    private bool IsAdmin() =>
        User.IsInRole("Admin");
    
}