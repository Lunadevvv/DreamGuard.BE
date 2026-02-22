using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class ProductVariantRepository : GenericRepository<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(DreamGuardContext context) : base(context)
        {
        }
    }
}