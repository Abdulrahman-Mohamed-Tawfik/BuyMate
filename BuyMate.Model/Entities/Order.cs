using BuyMate.Model.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuyMate.Model.Entities
{
    public class Order : ModifiableSoftDeleteEntity
    {
        public Guid UserId { get; set; }
        public Guid? CouponId { get; set; }
        public User? User { get; set; }
        public Coupon? Coupon { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public int OrderStatus { get; set; }// Pending:0 / Processing:1 / Shipped:2 / Delivered:3 / Canceled:4 / Returned:5
        public int PaymentStatus { get; set; }// Pending:0 / Completed:1 / Failed:2 / Refunded:3 / Canceled:4
        public string? TrackingNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Fees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
