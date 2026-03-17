using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ServicePackageService : IServicePackageService
    {
        private readonly IServicePackageRepository _repo;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        public ServicePackageService(IServicePackageRepository repo, IMapper mapper, ICloudinaryService cloudinaryService)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<Result<PaginatedList<ServicePackageResponse>>> GetAllByAdminAsync(int pageNumber, int pageSize, ServicePackageStatus status)
        {
            var servicePackage = await _repo.GetAllAdminAsync(pageNumber, pageSize, status);
            if (servicePackage == null || servicePackage.TotalCount == 0)
            {
                return Result<PaginatedList<ServicePackageResponse>>.Failure("No service package found", 404);
            }
            var servicePackageResponse = _mapper.Map<List<ServicePackageResponse>>(servicePackage.Items);
            var paginatedResult = new PaginatedList<ServicePackageResponse>(servicePackageResponse, servicePackage.TotalCount, servicePackage.PageNumber, servicePackage.PageSize);
            return Result<PaginatedList<ServicePackageResponse>>.Success(paginatedResult);
        }
        public async Task<Result<ServicePackageResponse>> GetByIdAsync(Guid servicePackageId)
        {
            var servicePackage = await _repo.GetByIdAsync(servicePackageId);
            if (servicePackage == null)
            {
                return Result<ServicePackageResponse>.Failure("service package not found", 404);
            }
            var servicePackageResponse = _mapper.Map<ServicePackageResponse>(servicePackage);
            return Result<ServicePackageResponse>.Success(servicePackageResponse);
        }
        public async Task<Result> CreateAsync(ServicePackageCreateRequest servicePackageRequest)
        {
            ServicePackage servicePackage = new ServicePackage
            {
                PackageName = servicePackageRequest.PackageName,
                Description = servicePackageRequest.Description,
                Price = servicePackageRequest.Price,
                Duration = servicePackageRequest.Duration,
                SuitableFor = servicePackageRequest.SuitableFor,
                Benefits = servicePackageRequest.Benefits,
                ServiceContent = servicePackageRequest.ServiceContent,
                status = servicePackageRequest.Status!.Value
            };
            var uploadImageResult = await _cloudinaryService.UploadImageAsync(servicePackageRequest.FormFile, "PACKAGE_FOLDER");
            servicePackage.ImageUrl = uploadImageResult.Data!.Url;
            servicePackage.PublicId = uploadImageResult.Data.PublicId;
            var result = await _repo.CreateAsync(servicePackage);

            return Result.Success($"{result}");
        }

        public async Task<Result> ReplaceImagesAsync(Guid servicePackageId, PackageImageUploadRequest file)
        {
            var servicePackage = await _repo.GetByIdAsync(servicePackageId);
            if (servicePackage == null)
            {
                return Result.Failure("Service package not found", 404);
            }
            var uploadImageResult = await _cloudinaryService.UploadImageAsync(file.FormFile, "PACKAGE_FOLDER");
            servicePackage.ImageUrl = uploadImageResult.Data!.Url;
            servicePackage.PublicId = uploadImageResult.Data.PublicId;
            var result = await _repo.UpdateAsync(servicePackage);
            return Result.Success($"{result}");
        }

        public async Task<Result> DeleteImageAsync(Guid servicePackageId)
        {
            var servicePackage = await _repo.GetByIdAsync(servicePackageId);
            if (servicePackage == null)
            {
                return Result.Failure("Service package not found", 404);
            }
            if (string.IsNullOrEmpty(servicePackage.PublicId) || string.IsNullOrEmpty(servicePackage.ImageUrl))
            {
                return Result.Failure("No image to delete", 400);
            }
            await _cloudinaryService.DeleteImageAsync(servicePackage.PublicId);
            servicePackage.ImageUrl = "";
            servicePackage.PublicId = "";
            var result = await _repo.UpdateAsync(servicePackage);
            return Result.Success($"{result}");
        }

        public async Task<Result> UpdateAsync(Guid servicePackageId, ServicePackageUpdateRequest servicePackageRequest)
        {
            var servicePackage = await _repo.GetByIdAsync(servicePackageId);
            if (servicePackage == null)
            {
                return Result.Failure("Service package not found", 404);
            }
            _mapper.Map(servicePackageRequest, servicePackage);
            var result = await _repo.UpdateAsync(servicePackage);
            return Result.Success($"{result}");
        }

        public async Task<Result> UpdateServicePackageStatusAsync(Guid servicePackageId, ServicePackageStatus status)
        {
            var servicePackage = await _repo.GetByIdAsync(servicePackageId);
            if (servicePackage == null)
            {
                return Result.Failure("Service package not found", 404);
            }
            servicePackage.status = status;
            var result = await _repo.UpdateAsync(servicePackage);
            return Result.Success($"{result}");
        }
    }
}
