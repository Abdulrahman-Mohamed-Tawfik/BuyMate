using Microsoft.AspNetCore.Identity;

namespace BuyMate.Model.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public int Gender { get; set; }
        public string? Address { get; set; }
        public string? ProfileImageUrl { get;  set; } = "UserProfileImages/Default.jpg";
        public string? Token { get; private set; }
        public string? PhoneOTP { get; private set; }
        public DateTimeOffset? PhoneOTPExpiryDate { get; private set; }
        public string? EmailOTP { get; private set; }
        public DateTimeOffset? EmailOTPExpiryDate { get; private set; }
        public bool IsDeleted { get; set; }
        public DateTime DeletedAt { get; set; }

        public Cart? Cart { get; set; }
        public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }
}
