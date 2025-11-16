using BuyMate.Model.Common;
using System.ComponentModel.DataAnnotations;

namespace BuyMate.Model.Entities
{
    public class ProductReview : ModifiableEntity
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string? Review { get; set; }


        public Product? Product { get; set; }
        public User? User { get; set; }
    }
}
