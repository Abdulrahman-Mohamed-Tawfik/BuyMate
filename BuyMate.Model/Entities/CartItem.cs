using BuyMate.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.Model.Entities
{
    public class CartItem : ModifiableEntity
    {


        [Required]
        public Guid CartId { get; set; }
        public Cart Cart { get; set; }


        [Required]
        public Guid ProductId { get; set; }
        public Product Product { get; set; }



        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue)]
        public decimal PriceAtAddition { get; set; }



        
        

    }
}
