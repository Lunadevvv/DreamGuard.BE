using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class AdminSearchServiceTaskRequest
    {
        public Guid? StaffId { get; set; }
        public Guid? SoId { get; set; }
    }
}
