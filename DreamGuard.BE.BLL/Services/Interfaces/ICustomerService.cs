using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<Result<CustomerResponse>> GetByIdAsync(Guid customerId);
        Task<Result<PaginatedList<CustomerResponse>>> GetPaginatedListAsync(int pageNumber, int pageSize, string searchName);
    }
}
