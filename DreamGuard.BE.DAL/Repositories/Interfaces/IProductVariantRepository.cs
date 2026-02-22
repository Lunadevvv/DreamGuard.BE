using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IProductVariantRepository : IGenericRepository<ProductVariant>
    {
        Task<List<ProductVariant>> GetVariantsByProductIdAsync(Guid productId);
        Task<ProductVariant?> GetVariantByIdAsync(Guid id);
    }
}