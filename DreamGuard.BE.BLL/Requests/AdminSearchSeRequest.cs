using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class AdminSearchSeRequest
    {
        public Guid? ServiceEvidenceId { get; set; }
        public Guid? ServiceTaskId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 4;
    }
}
