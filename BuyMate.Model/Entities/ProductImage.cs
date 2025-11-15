using BuyMate.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.Model.Entities
{
    public class ProductImage : ModifiableEntity
    {

        [Required]
        public Guid ProductId { get; set; }
        public Product Product { get; set; }



        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsMain { get; set; } = false;

      
    }
}
