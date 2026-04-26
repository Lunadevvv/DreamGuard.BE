using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class TradeInOrderImageCreateRequest
    {

        [MaxLength(5, ErrorMessage = "can't upload over 5 images")]
        public List<IFormFile>? Files { get; set; }
    }
}
