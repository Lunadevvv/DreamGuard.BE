using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class GetAllProductTypesRequest 
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public List<Guid> ExceedProductTypeIds { get; set; } = new List<Guid>();
}
}