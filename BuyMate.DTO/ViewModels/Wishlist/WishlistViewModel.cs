namespace BuyMate.DTO.ViewModels.Wishlist
{
    public class WishlistViewModel
    {
        public Guid Id { get; set; }
        public List<WishlistItemViewModel> Items { get; set; } = new List<WishlistItemViewModel>();
    }
}
