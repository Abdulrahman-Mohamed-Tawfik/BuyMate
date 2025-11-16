using System;
using System.Collections.Generic;

namespace BuyMate.DTO.ViewModels
{
 public enum OrderStatusViewModel
 {
 Pending,
 Confirmed,
 Shipped,
 Delivered,
 Cancelled
 }

 public class OrderViewModel
 {
 public Guid Id { get; set; }
 public DateTime OrderDate { get; set; }
 public OrderStatusViewModel Status { get; set; }
 public decimal Total { get; set; }
 public string? TrackingId { get; set; }
 public DateTime? EstimatedDelivery { get; set; }
 public DateTime? ActualDelivery { get; set; }
 public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
 public ShippingAddressViewModel Address { get; set; } = new ShippingAddressViewModel();
 }
}
