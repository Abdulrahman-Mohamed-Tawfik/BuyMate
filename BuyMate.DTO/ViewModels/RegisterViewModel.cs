using System.ComponentModel.DataAnnotations;

namespace BuyMate.DTO.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First name must contain letters only.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last name must contain letters only.")]
        public string LastName { get; set; } = string.Empty;


        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [RegularExpression(@"^(01)[0-9]{9}$", ErrorMessage = "Enter a valid phone number starting with 01 and contain 11 digit.")]

        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Password legnth must be at least 4.", MinimumLength = 4)]
        [RegularExpression(@"^(?=.*\d)(?=.*[^A-Za-z0-9]).{4,}$",
                                    ErrorMessage = "Password must contain at least one number and one special character.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
