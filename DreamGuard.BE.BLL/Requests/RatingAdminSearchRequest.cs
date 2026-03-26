using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class RatingAdminSearchRequest
    {
       public Guid? ServiceOrderId { get; set; }
       public Guid? StaffId { get; set; }
       public int? Score { get; set; }
       public DateTime? CreatedAt { get; set; }
       public DateTime? UpdatedAt { get; set; }
       public int PageNumber { get; set; } = 1;
       public int PageSize { get; set; } = 4;
    }
}
