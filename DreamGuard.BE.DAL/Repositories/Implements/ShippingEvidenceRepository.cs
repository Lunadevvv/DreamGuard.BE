using System;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class ShippingEvidenceRepository : GenericRepository<ShippingEvidence>, IShippingEvidenceRepository
    {
        public ShippingEvidenceRepository(DreamGuardContext context) : base(context)
        {
        }
    }
}
