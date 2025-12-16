using BuyMate.Model.Common;

namespace BuyMate.Model.Entities
{
    public class Wishlist : ModifiableEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
    }
}
