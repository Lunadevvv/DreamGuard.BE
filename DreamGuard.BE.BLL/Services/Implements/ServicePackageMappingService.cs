using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ServicePackageMappingService : IServicePackageMappingService
    {
        private readonly IServicePackageMappingRepository _servicePackageMappingRepository;
        private readonly IMapper _mapper;
        public ServicePackageMappingService(IServicePackageMappingRepository servicePackageMappingRepository, IMapper mapper)
        {
            _servicePackageMappingRepository = servicePackageMappingRepository;
            _mapper = mapper;
        }
        public async Task<Result<PaginatedList<ServicePackageMappingResponse>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var mappings = await _servicePackageMappingRepository.GetAllByAdminAsync(pageNumber, pageSize);
            var mappingResponse = _mapper.Map<List<ServicePackageMappingResponse>>(mappings.Items);
            var paginatedResult = new PaginatedList<ServicePackageMappingResponse>(mappingResponse, mappings.TotalCount, mappings.PageNumber, mappings.PageSize);
            return Result<PaginatedList<ServicePackageMappingResponse>>.Success(paginatedResult);
        }

        public async Task<Result<ServicePackageMappingResponse>> GetByIdAsync(Guid servicePackageMappingId)
        {
            var servicePackageMapping = await _servicePackageMappingRepository.GetByIdAsync(servicePackageMappingId);
            if (servicePackageMapping == null)
            {
                return Result<ServicePackageMappingResponse>.Failure("Service package mapping not found", 404);
            }
            var result = _mapper.Map<ServicePackageMappingResponse>(servicePackageMapping);
            return Result<ServicePackageMappingResponse>.Success(result);
        }

        public async Task<Result> UpdateByIdAsync(Guid servicePackageMappingId, ServicePackageMappingUpdateRequest servicePackageMappingUpdateRequest)
        {
            var servicePackageMapping = await _servicePackageMappingRepository.GetByIdAsync(servicePackageMappingId);
            if (servicePackageMapping == null)
            {
                return Result.Failure("Service package mapping not found", 404);
            }
            var update = _mapper.Map(servicePackageMappingUpdateRequest, servicePackageMapping);
            var result = await _servicePackageMappingRepository.UpdateAsync(update);
            return Result.Success($"{result}");
        }
    }
}
