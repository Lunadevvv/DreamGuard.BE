using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class ServiceOrderRepository : GenericRepository<ServiceOrder>, IServiceOrderRepository
    {
        public ServiceOrderRepository(DreamGuardContext context) : base(context)
        {
        }
        public async Task<PaginatedList<ServiceOrder>> GetAllAdminAsync(int pageNumber, int pageSize, string? orderCode, PaymentMethod? paymentMethod, PaymentStatus? paymentStatus)
        {
            try
            {
                var query = _context.ServiceOrders.Include(so => so.Payments).Include(so => so.ServiceTask)
                        .ThenInclude(st => st.Staff)
                            .ThenInclude(s => s.User)
                    .Where(so => (so.OrderCode == orderCode || string.IsNullOrEmpty(orderCode))
                    && (so.Payments.Any(p => (p.PaymentMethod == paymentMethod || paymentMethod == null) 
                    && (p.Status == paymentStatus || paymentStatus == null)))
                    );
                return await PaginatedList<ServiceOrder>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving ServiceOrder for admin");
            }
        }

        public async Task<PaginatedList<ServiceOrder>> GetAllAsync(int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.ServiceOrders
                    .Include(so => so.Payments)
                    .Include(so => so.ServiceTask)
                        .ThenInclude(st => st.Staff)
                            .ThenInclude(s => s.User);
                return await PaginatedList<ServiceOrder>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving ServiceOrder for customer");
            }
        }

        public async Task<ServiceOrder?> GetByIdWithDetail(Guid serviceOrderId)
        {
            try
            {
                return await _context.ServiceOrders
                     .Include(so => so.Payments)
                     .Include(so => so.ServiceOrderItems)
                            .ThenInclude(soi => soi.ServicePackageMapping)
                                 .ThenInclude(spm => spm.ServicePackage)
                     .Include(so => so.ServiceOrderItems)
                            .ThenInclude(soi => soi.ServicePackageMapping)
                                 .ThenInclude(spm => spm.ProductType)
                     .Include(so => so.ServiceAssets)
                     .Include(so => so.ServiceTask)
                        .ThenInclude(st => st.Staff)
                            .ThenInclude(s => s.User)
                     .FirstOrDefaultAsync(so => so.SoId == serviceOrderId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving ServiceOrder with detail");
            }
        }

        public async Task<ServiceOrder?> GetByIdWithServiceTask(Guid serviceOrderId)
        {
            try
            {
                return await _context.ServiceOrders
                     .Include(so => so.ServiceTask)
                     .Include(so => so.Payments)
                     .FirstOrDefaultAsync(so => so.SoId == serviceOrderId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving ServiceOrder with payment");
            }
        }
    }
}
