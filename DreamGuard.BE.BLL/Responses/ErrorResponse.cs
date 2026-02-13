using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ErrorResponse
    {
        public int ErrorCode { get; set; }
        public List<string> Message { get; set; } = new List<string>();
    }
}
