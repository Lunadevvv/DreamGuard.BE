using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class ProductCertificate
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
        [JsonIgnore]
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}