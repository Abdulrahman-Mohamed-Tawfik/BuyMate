using System;
using System.ComponentModel.DataAnnotations;

namespace BuyMate.DTO.ViewModels
{
    public class ProfileViewModel
    {
        public string Id { get; set; } = string.Empty;


        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name must not exceed 50 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First name must contain letters only.")]
        public string FirstName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name must not exceed 50 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last name must contain letters only.")]
        public string LastName { get; set; } = string.Empty;


        public string Name =>
            string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
            ? string.Empty
            : $"{FirstName} {LastName}";


        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^(01)[0-9]{9}$",
            ErrorMessage = "Phone must be an Egyptian number starting with 01 and containing 11 digits.")]
        public string Phone { get; set; } = string.Empty;


        [DataType(DataType.Date)]
        public DateOnly? BirthDate { get; set; }


        // ---------- Gender ----------
        // 0 = Unspecified, 1 = Male, 2 = Female
        [Range(0, 2, ErrorMessage = "Invalid gender selection.")]
        public int Gender { get; set; }


        [StringLength(200, ErrorMessage = "Address must not exceed 200 characters.")]
        public string? Address { get; set; }


        
        public string? Avatar { get; set; }


        public bool IsAdmin { get; set; }
    }
}
