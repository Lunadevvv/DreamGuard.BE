using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServicePackageRepository _servicePackageRepository;
        public ServicePackageMappingService(IServicePackageMappingRepository servicePackageMappingRepository, IMapper mapper, IUnitOfWork unitOfWork, IServicePackageRepository servicePackageRepository)
        {
            _servicePackageMappingRepository = servicePackageMappingRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _servicePackageRepository = servicePackageRepository;
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
        public async Task<Result<ServicePackageMappingResponse>> GetByProductTypeIdAndServicePackageIdAsync(Guid productTypeId, Guid servicePackageId)
        {
            var servicePackageMapping = await _servicePackageMappingRepository.GetByProductTypeIdAndServicePackageIdAsync(productTypeId, servicePackageId);
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
            if (servicePackageMappingUpdateRequest.ServicePackage != null)
            {
                var servicePackage = _mapper.Map(servicePackageMappingUpdateRequest.ServicePackage, servicePackageMapping.ServicePackage);
                _servicePackageRepository.UpdateEntity(servicePackage);

            }
            var update = _mapper.Map(servicePackageMappingUpdateRequest, servicePackageMapping);
            _servicePackageMappingRepository.UpdateEntity(update);
            var result = await _unitOfWork.SaveChangeAsync();
            return Result.Success($"{result}");
        }
    }
}
