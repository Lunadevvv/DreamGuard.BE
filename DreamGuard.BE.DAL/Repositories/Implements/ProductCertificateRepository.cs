using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class ProductCertificateRepository : GenericRepository<ProductCertificate>, IProductCertificateRepository
    {
        public ProductCertificateRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<PaginatedList<ProductCertificate>> GetAllAsync(int pageNumber, int pageSize)
        {
            var query = _context.ProductCertificates.AsQueryable();
            return await PaginatedList<ProductCertificate>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<List<ProductCertificate>> GetByProductCertificateIdsAsync(List<Guid> productCertificateIds)
        {
            return await _context.ProductCertificates
                .Where(c => productCertificateIds.Contains(c.Id))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<ProductCertificate>> GetByProductIdAsync(Guid productId)
        {
            return await _context.ProductCertificates
                .Where(c => c.IsActive && c.Products.Any(p => p.Id == productId))
                .ToListAsync();
        }
    }
}