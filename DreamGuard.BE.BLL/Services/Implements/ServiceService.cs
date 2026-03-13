using AutoMapper;
using CloudinaryDotNet.Actions;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;


namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _repo;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceAssetRepository _serviceAssetRepository;
        private readonly IServicePackageMappingRepository _servicePackageMappingRepository;
        private readonly IServicePackageRepository _servicePackageRepository;
      
        public ServiceService(IServiceRepository repo, IMapper mapper, ICloudinaryService cloudinaryService, IUnitOfWork unitOfWork, IServiceAssetRepository serviceAssetRepository, IServicePackageMappingRepository servicePackageMappingRepository, IServicePackageRepository servicePackageRepository)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
            _unitOfWork = unitOfWork;
            _serviceAssetRepository = serviceAssetRepository;
            _servicePackageMappingRepository = servicePackageMappingRepository;
            _servicePackageRepository = servicePackageRepository;
        }
        public async Task<Result<List<ServicePackageResponse>>> GetPackagesByServiceIdAsync(Guid serviceId)
        {
            var servicePackage = await _servicePackageRepository.GetAllByServiceIdAsync(serviceId);
            if (!servicePackage.Any())
            {
                return Result<List<ServicePackageResponse>>.Failure("service package not found", 404);
            }
            var servicePackageResponse = _mapper.Map<List<ServicePackageResponse>>(servicePackage);
            return Result<List<ServicePackageResponse>>.Success(servicePackageResponse);
        }
        public async Task<Result<PaginatedList<ServiceResponse>>> GetAllAsync(int pageNumber)
        {
            var services = await _repo.GetAllAsync(pageNumber);
            if (services == null || services.TotalCount == 0)
            {
                return Result<PaginatedList<ServiceResponse>>.Failure("No services found", 404);
            }
            var serviceResponse = _mapper.Map<List<ServiceResponse>>(services.Items);
            var paginatedResult = new PaginatedList<ServiceResponse>(serviceResponse, services.TotalCount, services.PageNumber, services.PageSize);
            return Result<PaginatedList<ServiceResponse>>.Success(paginatedResult);
        }

        public async Task<Result<PaginatedList<ServiceResponse>>> GetAllByAdminAsync(int pageNumber, bool isActive)
        {
            var services = await _repo.GetAllAdminAsync(pageNumber, isActive);
            if (services == null || services.TotalCount == 0)
            {
                return Result<PaginatedList<ServiceResponse>>.Failure("No services found", 404);
            }
            var serviceResponse = _mapper.Map<List<ServiceResponse>>(services.Items);
            var paginatedResult = new PaginatedList<ServiceResponse>(serviceResponse, services.TotalCount, services.PageNumber, services.PageSize);
            return Result<PaginatedList<ServiceResponse>>.Success(paginatedResult);
        }
        public async Task<Result<ServiceResponse>> GetByIdAsync(Guid serviceId)
        {
            var service = await _repo.GetByIdAsync(serviceId);
            if (service == null)
            {
                return Result<ServiceResponse>.Failure("service not found", 404);
            }
            var serviceResponse = _mapper.Map<ServiceResponse>(service);
            return Result<ServiceResponse>.Success(serviceResponse);
        }
        public async Task<Result> CreateAsync(ServiceCreateRequest service)
        {
            Service newService = new Service
            {
                ServiceName = service.ServiceName,
                Description = service.Description,
                Price = service.Price,
                IsActive = service.IsActive!.Value,
                EstimatedDuration = service.EstimatedDuration,
                CreatedAt = DateTime.UtcNow
            };
            foreach (var file in service.Files)
            {
                var uploadImageResult = await _cloudinaryService.UploadImageAsync(file, "SERVICE_FOLDER");
                var asset = new ServiceAsset
                {
                    ServiceId = newService.ServiceId,
                    Url = uploadImageResult.Data.Url,
                    Type = file.ContentType,
                    PublicId = uploadImageResult.Data.PublicId
                };
                newService.ServiceAssets.Add(asset);
            }
            var result = await _repo.CreateAsync(newService);

            return Result.Success($"{result}");
        }

        public async Task<Result> AddImagesAsync(Guid serviceId, ImageUploadRequest files)
        {
            var service = await _repo.GetByIdAsync(serviceId);
            if (service == null)
            {
                return Result.Failure("Service not found", 404);
            }
            foreach (var file in files.Files)
            {
                var uploadImageResult = await _cloudinaryService.UploadImageAsync(file, "SERVICE_FOLDER");
                if (!uploadImageResult.Succeeded)
                {
                    return Result.Failure($"Failed to upload image: {uploadImageResult.Error}", 500);
                }
                var asset = new ServiceAsset
                {
                    ServiceId = serviceId,
                    Url = uploadImageResult.Data.Url,
                    Type = file.ContentType,
                    PublicId = uploadImageResult.Data.PublicId
                };
                _serviceAssetRepository.AddEntity(asset);
            }
            var result = await _unitOfWork.SaveChangeAsync();
            return Result.Success($"{result}");
        }

        public async Task<Result> DeleteImageAsync(Guid serviceId, Guid assetId)
        {
            var asset = await _serviceAssetRepository.GetByIdAsync(assetId);
            if (asset == null || asset.ServiceId != serviceId)
            {
                return Result.Failure("Image not found", 404);
            }
            await _cloudinaryService.DeleteImageAsync(asset.PublicId);
            var result = await _serviceAssetRepository.RemoveAsync(asset);
            return Result.Success($"{result}");
        }

        public async Task<Result> UpdateAsync(Guid serviceId, ServiceUpdateRequest serviceRequest)
        {
            var existingService = await _repo.GetByIdAsync(serviceId);
            if (existingService == null)
            {
                return Result.Failure("Service not found", 404);
            }
            _mapper.Map(serviceRequest, existingService);
            existingService.UpdatedAt = DateTime.UtcNow;
            var result = await _repo.UpdateAsync(existingService);
            return Result.Success($"{result}");
        }

        public async Task<Result> ToggleActiveAsync(Guid serviceId)
        {
            var service = await _repo.GetByIdAsync(serviceId);
            if (service == null)
            {
                return Result.Failure("Service not found", 404);
            }
            service.IsActive = !service.IsActive;
            var result = await _repo.UpdateAsync(service);
            return Result.Success($"{result}");
        }

        public async Task<Result> AssignPackagesAsync(Guid serviceId, AssignServicePackagesRequest assignServicePackagesRequest)
        {
            //check if service exist
            var service = await _repo.GetByIdAsync(serviceId);
            if (service == null)
            {
                return Result.Failure("Service not found", 404);
            }
            //check if all package exist
            var packageList = await _servicePackageRepository.GetByListIdAsync(assignServicePackagesRequest.ServicePackageIds);
            if (packageList.Count < assignServicePackagesRequest.ServicePackageIds.Count)
            {
                return Result.Failure($"Some ServicePackage not found", 404);
            }
            //check if mapping already exists with the same serviceId and packageId
            var isMappingExist = await _servicePackageMappingRepository.CheckMappingExistAsync(serviceId, assignServicePackagesRequest.ServicePackageIds);
            if (isMappingExist)
            {
                return Result.Failure($"Mapping already exists for some ServicePackage", 400);
            }
            foreach (var packageId in assignServicePackagesRequest.ServicePackageIds)
            {

                ServicePackageMapping mapping = new ServicePackageMapping
                {
                    ServiceId = serviceId,
                    ServicePackageId = packageId,
                    Duration = service.EstimatedDuration + packageList.FirstOrDefault(p => p.ServicePackageId == packageId)!.Duration,
                    Price = service.Price + packageList.FirstOrDefault(p => p.ServicePackageId == packageId)!.Price
                };
                _servicePackageMappingRepository.AddEntity(mapping);
            }
            var result = await _unitOfWork.SaveChangeAsync();
            return Result.Success($"{result}");
        }
        public async Task<Result> RemovePackagesAsync(Guid serviceId, RemoveServicePackagesRequest removeServicePackagesRequest)
        {
            //check if service exist
            var service = await _repo.GetByIdAsync(serviceId);
            if (service == null)
            {
                return Result.Failure("Service not found", 404);
            }
            //check if all package exist
            var packageList = await _servicePackageRepository.GetByListIdAsync(removeServicePackagesRequest.ServicePackageIds);
            if (packageList.Count < removeServicePackagesRequest.ServicePackageIds.Count)
            {
                return Result.Failure($"Some ServicePackage not found", 404);
            }
            //check if mapping already exists with the same serviceId and packageId
            var mappingList = await _servicePackageMappingRepository.GetListByIdAsync(serviceId, removeServicePackagesRequest.ServicePackageIds);
            if (mappingList.Count < removeServicePackagesRequest.ServicePackageIds.Count)
            {
                return Result.Failure($"Mapping not exists for some ServicePackage", 400);
            }
            foreach (var mapping in mappingList)
            {
                _servicePackageMappingRepository.RemoveEntity(mapping);
            }
            var result = await _unitOfWork.SaveChangeAsync();
            return Result.Success($"{result}");
        }
    }
}