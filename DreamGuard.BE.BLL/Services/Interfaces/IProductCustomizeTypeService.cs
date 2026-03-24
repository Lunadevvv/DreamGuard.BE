using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IProductCustomizeTypeService
    {
        Task<Result<ProductCustomizeType>> GetProductCustomizeTypeByIdAsync(Guid id);
        Task<Result<PaginatedList<ProductCustomizeType>>> GetProductCustomizeTypesAsync(int pageNumber, int pageSize, List<Guid> exceedProductCustomizeIds);
        Task<Result> CreateProductCustomizeTypeAsync(ProductCustomizeType customizeType);
        Task<Result> UpdateProductCustomizeTypeAsync(Guid id, ProductCustomizeType customizeType);
    }
}