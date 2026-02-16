using System.Runtime.CompilerServices;

namespace StoreApi.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }

    
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled
    public string ShippingAddress { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Payment properties
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
    public string? TransactionId { get; set; }
    
    // Navigation properties
    public User? User { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
