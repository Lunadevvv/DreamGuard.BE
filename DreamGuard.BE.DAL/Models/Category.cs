using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class Category
    {
        public int CateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string slug { get; set; } = string.Empty;
        public int? CateParentId { get; set; }
        [JsonIgnore]
        public Category? CateParent { get; set; }
        [JsonIgnore]
        public ICollection<Category> ChildCategoryList { get; set; } = new List<Category>();
        [JsonIgnore]
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}