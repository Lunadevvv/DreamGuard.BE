using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class VerifyOtpRequest
    {
        [Required(ErrorMessage = "Phone number is required.")]
        public string phoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string email { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP code is required.")]
        public string OtpCode { get; set; } = string.Empty;
    }
}
