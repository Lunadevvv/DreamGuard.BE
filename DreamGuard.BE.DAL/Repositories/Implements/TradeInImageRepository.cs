using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class TradeInImageRepository : GenericRepository<TradeInImage>, ITradeInImageRepository
    {
        public TradeInImageRepository(DreamGuardContext context) : base(context)
        {
        }
    }
}
