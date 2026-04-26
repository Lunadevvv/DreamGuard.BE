using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class CalculateTradeInOrderPriceRequest
    {
        public Guid OldProductVariantId { get; set; }
        public Guid ProductVariantId { get; set; }
    }
}
