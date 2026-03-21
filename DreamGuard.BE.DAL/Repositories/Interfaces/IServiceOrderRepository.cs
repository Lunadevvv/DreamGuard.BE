using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IServiceOrderRepository : IGenericRepository<ServiceOrder>
    {
        Task<PaginatedList<ServiceOrder>> GetAllAdminAsync(int pageNumber, int pageSize, string? orderCode, PaymentMethod? paymentMethod, PaymentStatus? paymentStatus);
        Task<PaginatedList<ServiceOrder>> GetAllAsync(int pageNumber, int pageSize);
        Task<ServiceOrder?> GetByIdWithDetail(Guid serviceOrderId);
        Task<ServiceOrder?> GetByIdWithServiceTask(Guid serviceOrderId);
    }
}
