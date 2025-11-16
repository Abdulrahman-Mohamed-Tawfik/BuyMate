using BuyMate.Model.Common;
using System.ComponentModel.DataAnnotations;

namespace BuyMate.Model.Entities
{
    public class Cart : ModifiableEntity
    {
        [Required]
        public Guid UserId { get; set; }

        public User? User { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
