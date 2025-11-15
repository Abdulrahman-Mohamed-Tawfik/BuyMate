using BuyMate.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.Model.Entities
{
    public class Order : ModifiableEntity
    {
        public Guid UserID { get; set; }
        public User User { get; set; }


        public Guid? CouponId { get; set; }
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
