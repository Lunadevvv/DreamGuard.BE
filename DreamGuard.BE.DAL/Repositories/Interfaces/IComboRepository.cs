using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IComboRepository : IGenericRepository<Combo>
    {
        Task<PaginatedList<Combo>> GetAllCombosAsync(
            int pageNumber, double? maxPrice, int? maxAgeGroup, string? color);
        Task<Combo?> GetComboByIdAsync(Guid id);
        Task<Combo?> GetComboWithChildrenAsync(Guid id, string? size, string? color);
        Task<Combo?> GetComboWithProductsAsync(Guid id);
        Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null);
        Task RemoveComboProductVariantsAsync(Guid comboId);
        Task AddComboProductVariantsAsync(List<ComboProductVariant> items);
    }
}
