using BuyMate.Model.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuyMate.Model.Entities
{
    public class Order : ModifiableSoftDeleteEntity
    {
        public Guid UserId { get; set; }
        public Guid? CouponId { get; set; }
        public User User { get; set; }
        public Coupon? Coupon { get; set; }



        public string ShippingAddress { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

     
        // Pending / Processing / Shipped / Delivered / Cancelled / Returned
        public string OrderStatus { get; set; } = "Pending";

   
        // Unpaid / Paid / Refunded / Failed / PendingReview
        public string PaymentStatus { get; set; } = "Unpaid";

        public string? TrackingNumber { get; set; }



        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
