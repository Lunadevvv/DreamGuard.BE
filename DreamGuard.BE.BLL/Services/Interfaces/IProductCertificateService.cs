using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IProductCertificateService
    {
        Task<Result<ProductCertificate>> CreateAsync(ProductCertificateCreateRequest request);
        Task<Result<PaginatedList<ProductCertificate>>> GetAllAsync(int pageNumber, int pageSize);
        Task<Result<ProductCertificate>> GetByIdAsync(Guid id);
        Task<Result<List<ProductCertificate>>> GetByProductIdAsync(Guid productId);
        Task<Result<ProductCertificate>> UpdateAsync(Guid id, ProductCertificateUpdateRequest request);
        Task<Result> UpdateStatusAsync(Guid id, bool isActive);
    }
}