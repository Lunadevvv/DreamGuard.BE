using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IProductCertificateRepository : IGenericRepository<ProductCertificate>
    {
        Task<PaginatedList<ProductCertificate>> GetAllAsync(int pageNumber, int pageSize);
        Task<List<ProductCertificate>> GetByProductIdAsync(Guid productId);
        Task<List<ProductCertificate>> GetByProductCertificateIdsAsync(List<Guid> productCertificateIds);
    }
}