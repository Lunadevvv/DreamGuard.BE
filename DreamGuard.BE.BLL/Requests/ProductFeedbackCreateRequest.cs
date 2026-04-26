using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class ProductFeedbackCreateRequest
    {
        [Range(1, 5, ErrorMessage = "Score must be between 1 and 5.")]
        public int Score { get; set; }

        [Required(ErrorMessage = "Comment is required.")]
        public string Comment { get; set; } = string.Empty;
    }
}
