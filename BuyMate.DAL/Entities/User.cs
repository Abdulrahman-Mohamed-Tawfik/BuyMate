using Microsoft.AspNetCore.Identity;

namespace BuyMate.DAL.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public int Gender { get; set; }
        public string? Address { get; set; }
        public string? ProfileImageUrl { get; private set; } = "UserAvatars/Default.png";
        public string? Token { get; private set; }
        public string? PhoneOTP { get; private set; }
        public DateTimeOffset? PhoneOTPExpiryDate { get; private set; }
        public string? EmailOTP { get; private set; }
        public DateTimeOffset? EmailOTPExpiryDate { get; private set; }
        public bool IsDeleted { get; set; }
    }
}
