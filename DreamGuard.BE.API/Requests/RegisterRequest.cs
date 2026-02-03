using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.API.Dtos
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).*$",ErrorMessage = "Mật khẩu phải có chữ hoa, chữ thường, số, ký tự đặc biệt")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vui lòng nhập tên người dùng")]
        public string UserName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(94|0)(3|5|7|8|9)\d{8}$", ErrorMessage = "Số điện thoại không đúng định dạng Việt Nam")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vui lòng nhập giới tính")]
        public string Gender { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        public DateOnly DateOfBirth { get; set; } 
    }
}
