using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.API.Dtos
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = string.Empty;
    }
}
