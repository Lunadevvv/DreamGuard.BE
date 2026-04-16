using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class TotalAmountLineChartResponse
    {
        public decimal TotalAmount { get; set; }
        public DateOnly Date { get; set; }
    }
}
