using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Responses
{
    public class TopProductSeller
    {
        public Product Product { get; set; }
        public int TotalQuantity { get; set; }
    }
}
