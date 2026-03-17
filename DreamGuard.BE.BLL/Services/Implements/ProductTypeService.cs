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
    public class ProductTypeService : IProductTypeService
    {
        private readonly IProductTypeRepository _repo;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServicePackageMappingRepository _servicePackageMappingRepository;
        private readonly IServicePackageRepository _servicePackageRepository;
   
        

        public ProductTypeService(IProductTypeRepository repo, IMapper mapper, ICloudinaryService cloudinaryService, IUnitOfWork unitOfWork, IServicePackageMappingRepository servicePackageMappingRepository, IServicePackageRepository servicePackageRepository)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
            _unitOfWork = unitOfWork;
            _servicePackageMappingRepository = servicePackageMappingRepository;
            _servicePackageRepository = servicePackageRepository;
        }


        public async Task<Result<List<ServicePackageMappingDetailResponse>>> GetMappingsByProductTypeIdAsync(Guid productTypeId)
        {
            var servicePackageMapping = await _repo.GetMappingsByProductTypeIdAsync(productTypeId);
            if (!servicePackageMapping.Any())
            {
                return Result<List<ServicePackageMappingDetailResponse>>.Failure("Service mapping not found", 404);
            }
            var servicePackageResponse = _mapper.Map<List<ServicePackageMappingDetailResponse>>(servicePackageMapping);
            return Result<List<ServicePackageMappingDetailResponse>>.Success(servicePackageResponse);
        }
        public async Task<Result<List<ServicePackageResponse>>> GetPackagesByProductTypeIdAsync(Guid productTypeId)
        {
            var servicePackage = await _servicePackageRepository.GetAllByProductTypeIdAsync(productTypeId);
            if (!servicePackage.Any())
            {
                return Result<List<ServicePackageResponse>>.Failure("service package not found", 404);
            }
            var servicePackageResponse = _mapper.Map<List<ServicePackageResponse>>(servicePackage);
            return Result<List<ServicePackageResponse>>.Success(servicePackageResponse);
        }
        public async Task<Result<PaginatedList<ProductTypeResponse>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var productTypes = await _repo.GetAllAsync(pageNumber, pageSize);
            if (productTypes == null || productTypes.TotalCount == 0)
            {
                return Result<PaginatedList<ProductTypeResponse>>.Failure("No productType found", 404);
            }
            var productTypeResponse = _mapper.Map<List<ProductTypeResponse>>(productTypes.Items);
            var paginatedResult = new PaginatedList<ProductTypeResponse>(productTypeResponse, productTypes.TotalCount, productTypes.PageNumber, productTypes.PageSize);
            return Result<PaginatedList<ProductTypeResponse>>.Success(paginatedResult);
        }

        public async Task<Result<PaginatedList<ProductTypeResponse>>> GetAllByAdminAsync(int pageNumber, int pageSize, bool isActive)
        {
            var productTypes = await _repo.GetAllAdminAsync(pageNumber, pageSize, isActive);
            if (productTypes == null || productTypes.TotalCount == 0)
            {
                return Result<PaginatedList<ProductTypeResponse>>.Failure("No productTypes found", 404);
            }
            var productTypeResponse = _mapper.Map<List<ProductTypeResponse>>(productTypes.Items);
            var paginatedResult = new PaginatedList<ProductTypeResponse>(productTypeResponse, productTypes.TotalCount, productTypes.PageNumber, productTypes.PageSize);
            return Result<PaginatedList<ProductTypeResponse>>.Success(paginatedResult);
        }
        public async Task<Result<ProductTypeResponse>> GetByIdAsync(Guid productTypeId)
        {
            var productType = await _repo.GetByIdAsync(productTypeId);
            if (productType == null)
            {
                return Result<ProductTypeResponse>.Failure("productType not found", 404);
            }
            var productTypeResponse = _mapper.Map<ProductTypeResponse>(productType);
            return Result<ProductTypeResponse>.Success(productTypeResponse);
        }
        public async Task<Result> CreateAsync(ProductTypeCreateRequest productType)
        {
            ProductType newProductType = new ProductType
            {
                ProductTypeName = productType.ProductTypeName,
                Price = productType.Price,
                IsActive = productType.IsActive!.Value,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _repo.CreateAsync(newProductType);

            return Result.Success($"{result}");
        }


        public async Task<Result> UpdateAsync(Guid productTypeId, ProductTypeUpdateRequest productTypeRequest)
        {
            var existingProductType = await _repo.GetByIdAsync(productTypeId);
            if (existingProductType == null)
            {
                return Result.Failure("productType not found", 404);
            }
            _mapper.Map(productTypeRequest, existingProductType);
            var result = await _repo.UpdateAsync(existingProductType);
            return Result.Success($"{result}");
        }

        public async Task<Result> ToggleActiveAsync(Guid productTypeId)
        {
            var productType = await _repo.GetByIdAsync(productTypeId);
            if (productType == null)
            {
                return Result.Failure("ProductType not found", 404);
            }
            productType.IsActive = !productType.IsActive;
            var result = await _repo.UpdateAsync(productType);
            return Result.Success($"{result}");
        }

        public async Task<Result> AssignPackagesAsync(Guid productTypeId, AssignServicePackagesRequest assignServicePackagesRequest)
        {
            //check if productType exist
            var productType = await _repo.GetByIdAsync(productTypeId);
            if (productType == null)
            {
                return Result.Failure("productType not found", 404);
            }
            //check if all package exist
            var packageList = await _servicePackageRepository.GetByListIdAsync(assignServicePackagesRequest.ServicePackageIds);
            if (packageList.Count < assignServicePackagesRequest.ServicePackageIds.Count)
            {
                return Result.Failure($"Some ServicePackage not found", 404);
            }
            //check if mapping already exists with the same productTypeId and packageId
            var isMappingExist = await _servicePackageMappingRepository.CheckMappingExistAsync(productTypeId, assignServicePackagesRequest.ServicePackageIds);
            if (isMappingExist)
            {
                return Result.Failure($"Mapping already exists for some ServicePackage", 400);
            }
            foreach (var packageId in assignServicePackagesRequest.ServicePackageIds)
            {

                ServicePackageMapping mapping = new ServicePackageMapping
                {
                    ProductTypeId = productTypeId,
                    ServicePackageId = packageId,
                    Duration = packageList.FirstOrDefault(p => p.ServicePackageId == packageId)!.Duration,
                    Price = productType.Price + packageList.FirstOrDefault(p => p.ServicePackageId == packageId)!.Price
                };
                _servicePackageMappingRepository.AddEntity(mapping);
            }
            var result = await _unitOfWork.SaveChangeAsync();
            return Result.Success($"{result}");
        }
        public async Task<Result> RemovePackagesAsync(Guid productTypeId, RemoveServicePackagesRequest removeServicePackagesRequest)
        {
            //check if productType exist
            var productType = await _repo.GetByIdAsync(productTypeId);
            if (productType == null)
            {
                return Result.Failure("productType not found", 404);
            }
            //check if all package exist
            var packageList = await _servicePackageRepository.GetByListIdAsync(removeServicePackagesRequest.ServicePackageIds);
            if (packageList.Count < removeServicePackagesRequest.ServicePackageIds.Count)
            {
                return Result.Failure($"Some ServicePackage not found", 404);
            }
            //check if mapping already exists with the same productTypeId and packageId
            var mappingList = await _servicePackageMappingRepository.GetListByIdAsync(productTypeId, removeServicePackagesRequest.ServicePackageIds);
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