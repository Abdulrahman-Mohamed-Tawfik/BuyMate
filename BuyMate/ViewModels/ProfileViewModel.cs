using System.ComponentModel.DataAnnotations;

namespace BuyMate.ViewModels
{
    public class ProfileViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public string? Avatar { get; set; }

        public bool IsAdmin { get; set; }
    }
}
