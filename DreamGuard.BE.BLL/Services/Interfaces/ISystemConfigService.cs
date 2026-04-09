using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.ModelExtensions;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface ISystemConfigService
    {
        Task<Result<SystemConfigResponse>> GetConfigByKeyAsync(string key);
        Task<Result<PaginatedList<SystemConfigResponse>>> GetAllConfigsAsync(int pageNumber, int pageSize);
        Task<Result> CreateConfigAsync(SystemConfigCreateRequest request);
        Task<Result> UpdateConfigAsync(string key, SystemConfigUpdateRequest request);
        Task<Result> DeleteConfigAsync(string key);
    }
}
