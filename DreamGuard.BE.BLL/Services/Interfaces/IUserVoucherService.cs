using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IUserVoucherService
    {
        Task<Result<PaginatedList<UserVoucherResponse>>> GetAllByUserAsync(Guid userId, int pageNumber, bool? isUsed = null);
    }
}