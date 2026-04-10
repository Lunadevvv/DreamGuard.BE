using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class TradeInImage
    {
        public Guid TradeInImageId { get; set; } = Guid.NewGuid();
        public Guid TradeInOrderId { get; set; }
        public string ImageUrl { get; set; } 
        public string PublicId { get; set; }
        [JsonIgnore]
        public TradeInOrder TradeInOrder { get; set; } 
    }
}
