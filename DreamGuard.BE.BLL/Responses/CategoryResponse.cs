using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class CategoryResponse
    {
        public int CateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string slug { get; set; } = string.Empty;
        public ICollection<CategoryResponse> ChildCategoryList { get; set; } = new List<CategoryResponse>();
    }
}