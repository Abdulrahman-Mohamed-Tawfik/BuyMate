using System;

namespace BuyMate.DTO.ViewModels
{
 public class OrderItemViewModel
 {
 public Guid ProductId { get; set; }
 public string ProductName { get; set; } = string.Empty;
 public string? ImageUrl { get; set; }
 public string? Brand { get; set; }
 public int Quantity { get; set; }
 public decimal UnitPrice { get; set; }
 public decimal TotalPrice => UnitPrice * Quantity;
 }
}
