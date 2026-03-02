using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DreamGuard.BE.BLL.Requests
{
    public class UploadProductImageRequest
    {
        public IFormFile File { get; set; }
    }
}