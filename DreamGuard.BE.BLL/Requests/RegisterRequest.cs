using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email cannot be empty")]
        [EmailAddress(ErrorMessage = "Email doesn't have correct format")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password cannot be empty")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).*$",ErrorMessage = "Password must contain uppercase letter, lowercase letter, digit, and special character")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "First name cannot be empty")]
        public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Last name cannot be empty")]
        public string LastName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone number cannot be empty")]
        [RegularExpression(@"^(94|0)(3|5|7|8|9)\d{8}$", ErrorMessage = "Phone number doesn't have correct format")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Gender cannot be empty")]
        public string Gender { get; set; } = string.Empty;
        [Required(ErrorMessage = "Date of birth cannot be empty")]
        public DateOnly DateOfBirth { get; set; } 
    }
}
