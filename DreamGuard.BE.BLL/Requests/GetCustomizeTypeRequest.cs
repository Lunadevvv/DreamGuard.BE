using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DreamGuard.BE.BLL.Requests
{
    public class GetCustomizeTypeRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public List<Guid> ExceedProductCustomizeIds { get; set; } = new List<Guid>();
    }
}