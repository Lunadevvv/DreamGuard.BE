using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.API.Dtos
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Phone number cannot be empty")]
        [RegularExpression(@"^(94|0)(3|5|7|8|9)\d{8}$", ErrorMessage = "Phone number doesn't have correct format")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = string.Empty;
    }
}
