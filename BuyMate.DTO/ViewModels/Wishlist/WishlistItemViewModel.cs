namespace BuyMate.DTO.ViewModels.Wishlist
{
    public class WishlistItemViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int Rating { get; set; }
        public int ReviewCount { get; set; }
    }
}
