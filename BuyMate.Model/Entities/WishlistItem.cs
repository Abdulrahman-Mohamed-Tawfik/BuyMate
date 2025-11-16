using BuyMate.Model.Common;

namespace BuyMate.Model.Entities
{
    public class WishlistItem : ModifiableEntity
    {
        public Guid WishlistId { get; set; }
        public Guid ProductId { get; set; }


        public Wishlist Wishlist { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
