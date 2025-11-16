using BuyMate.Model.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuyMate.Model.Entities
{
    public class CartItem : ModifiableEntity
    {
        [Required]
        public Guid CartId { get; set; }


        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue)]
        public decimal PriceAtAddition { get; set; }


        public Cart Cart { get; set; }
        public Product Product { get; set; }
    }
}
