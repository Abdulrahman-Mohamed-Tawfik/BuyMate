using BuyMate.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.Model.Entities
{
    public class ProductReview : ModifiableEntity
    {

        [Required]
        public Guid ProductId { get; set; }
        public Product Product { get; set; }


        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; }



        public int Rating {  get; set; }
        public string Review { get; set; }


    }
}
