using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class SearchServiceByAdminRequest
    { 
        public int pageNumber { get; set; } = 1;
        public int pageSize { get; set; } = 4;
        public bool isActive { get; set; } = true;
    }
}
