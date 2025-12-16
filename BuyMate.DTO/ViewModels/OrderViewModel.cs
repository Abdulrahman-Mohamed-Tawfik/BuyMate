using BuyMate.Model.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuyMate.DTO.ViewModels
{
    public enum OrderStatusViewModel
    {
        Pending = 0,
        Confirmed = 1,
        Processing = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4,
        Returned = 5
    }

    public enum PaymentStatusViewModel
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Refunded = 3,
        Cancelled = 4
    }

    public class OrderViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? CouponId { get; set; }

        // ShippingAddress stored as a JSON/string in the entity; keep a string here for parity
        public string ShippingAddress { get; set; } = string.Empty;

        // Provide an Address view model for views that expect structured address
        public ShippingAddressViewModel Address { get; set; } = new ShippingAddressViewModel();

        public DateTime OrderDate { get; set; }

        // Match the entity: integer status fields
        public int OrderStatus { get; set; }

        // Enum alias used by views; maps to OrderStatus integer
        public OrderStatusViewModel Status
        {
            get => (OrderStatusViewModel)OrderStatus;
            set => OrderStatus = (int)value;
        }

        // Match the entity: integer payment status
        public int PaymentStatus { get; set; }

        // Enum alias for convenience
        public PaymentStatusViewModel PaymentStatusEnum
        {
            get => (PaymentStatusViewModel)PaymentStatus;
            set => PaymentStatus = (int)value;
        }

        // Entity uses TrackingNumber; provide alias TrackingId for views
        public string? TrackingNumber { get; set; }
        public string? TrackingId
        {
            get => TrackingNumber;
            set => TrackingNumber = value;
        }

        // Optional delivery fields (kept for UI display)
        public DateTime? EstimatedDelivery { get; set; }
        public DateTime? ActualDelivery { get; set; }

        public decimal Subtotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal Fees { get; set; }

        public decimal Total { get; set; }

        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();


        
    }
}
