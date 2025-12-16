using BuyMate.Model.Common;
using System.ComponentModel.DataAnnotations;

namespace BuyMate.Model.Entities
{
    public class ProductImage : ModifiableEntity
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; } = false;

        public Product? Product { get; set; }
    }
}
