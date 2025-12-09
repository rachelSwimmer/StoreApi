using StoreApi.DTOs;
using StoreApi.Interfaces;
using StoreApi.Models;

namespace StoreApi.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<OrderService> _logger;
    
    public OrderService(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IProductRepository productRepository,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _logger = logger;
    }
    
    public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Select(MapToResponseDto);
    }
    
    public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        return order != null ? MapToResponseDto(order) : null;
    }
    
    public async Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return orders.Select(MapToResponseDto);
    }
    
    public async Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto createDto)
    {
        if (!await _userRepository.ExistsAsync(createDto.UserId))
        {
            throw new ArgumentException($"User with ID {createDto.UserId} does not exist.");
        }
        
        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;
        
        foreach (var itemDto in createDto.OrderItems)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {itemDto.ProductId} does not exist.");
            }
            
            if (product.Stock < itemDto.Quantity)
            {
                throw new ArgumentException($"Insufficient stock for product {product.Name}. Available: {product.Stock}");
            }
            
            var subtotal = product.Price * itemDto.Quantity;
            totalAmount += subtotal;
            
            orderItems.Add(new OrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price,
                Subtotal = subtotal
            });
            
            // Update product stock
            product.Stock -= itemDto.Quantity;
            await _productRepository.UpdateAsync(product);
        }
        
        var order = new Order
        {
            UserId = createDto.UserId,
            ShippingAddress = createDto.ShippingAddress,
            TotalAmount = totalAmount,
            Status = "Pending",
            OrderItems = orderItems
        };
        
        var createdOrder = await _orderRepository.CreateAsync(order);
        _logger.LogInformation("Order created with ID: {OrderId}", createdOrder.Id);
        
        // Reload to get navigation properties
        var orderWithDetails = await _orderRepository.GetByIdAsync(createdOrder.Id);
        return MapToResponseDto(orderWithDetails!);
    }
    
    public async Task<OrderResponseDto?> UpdateOrderAsync(int id, OrderUpdateDto updateDto)
    {
        var existingOrder = await _orderRepository.GetByIdAsync(id);
        if (existingOrder == null) return null;
        
        if (updateDto.ShippingAddress != null)
        {
            existingOrder.ShippingAddress = updateDto.ShippingAddress;
        }
        
        if (updateDto.Status != null)
        {
            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(updateDto.Status))
            {
                throw new ArgumentException($"Invalid status. Valid values are: {string.Join(", ", validStatuses)}");
            }
            
            existingOrder.Status = updateDto.Status;
            
            if (updateDto.Status == "Shipped" && !existingOrder.ShippedDate.HasValue)
            {
                existingOrder.ShippedDate = DateTime.UtcNow;
            }
            
            if (updateDto.Status == "Delivered" && !existingOrder.DeliveredDate.HasValue)
            {
                existingOrder.DeliveredDate = DateTime.UtcNow;
            }
        }
        
        var updatedOrder = await _orderRepository.UpdateAsync(existingOrder);
        return updatedOrder != null ? MapToResponseDto(updatedOrder) : null;
    }
    
    public async Task<bool> DeleteOrderAsync(int id)
    {
        return await _orderRepository.DeleteAsync(id);
    }
    
    private static OrderResponseDto MapToResponseDto(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            UserName = order.User != null ? $"{order.User.FirstName} {order.User.LastName}" : string.Empty,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            ShippingAddress = order.ShippingAddress,
            OrderDate = order.OrderDate,
            ShippedDate = order.ShippedDate,
            DeliveredDate = order.DeliveredDate,
            OrderItems = order.OrderItems?.Select(oi => new OrderItemResponseDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? string.Empty,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Subtotal = oi.Subtotal
            }).ToList() ?? new List<OrderItemResponseDto>()
        };
    }
}
