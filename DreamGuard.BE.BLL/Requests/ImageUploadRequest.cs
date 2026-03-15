using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class ImageUploadRequest
    {
        [MinLength(1, ErrorMessage = "At least one file is required.")]
        public ICollection<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}
