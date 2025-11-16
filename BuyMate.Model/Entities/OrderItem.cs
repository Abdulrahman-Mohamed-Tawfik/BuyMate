using BuyMate.Model.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuyMate.Model.Entities
{
    public class OrderItem : ModifiableSoftDeleteEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
