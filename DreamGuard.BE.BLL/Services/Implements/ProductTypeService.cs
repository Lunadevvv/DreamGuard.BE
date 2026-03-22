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
            var servicePackageResponse = _mapper.Map<List<ServicePackageMappingDetailResponse>>(servicePackageMapping);
            return Result<List<ServicePackageMappingDetailResponse>>.Success(servicePackageResponse);
        }
    
        public async Task<Result<List<ServicePackageResponse>>> GetPackagesByProductTypeIdsAsync(List<Guid> productTypeIds)
        {
            var servicePackage = await _servicePackageRepository.GetAllByProductTypeIdsAsync(productTypeIds);
            var servicePackageResponse = _mapper.Map<List<ServicePackageResponse>>(servicePackage);
            return Result<List<ServicePackageResponse>>.Success(servicePackageResponse);
        }
        public async Task<Result<List<ServicePackageResponse>>> GetAllPackageByProductTypeIdAsync(Guid productTypeId)
        {
            var servicePackage =  await _servicePackageRepository.GetAllByProductTypeIdAsync(productTypeId);
            var servicePackageResponse = _mapper.Map<List<ServicePackageResponse>>(servicePackage);
            return Result<List<ServicePackageResponse>>.Success(servicePackageResponse);
        }
        public async Task<Result<PaginatedList<ProductTypeResponse>>> GetAllAsync(int pageNumber, int pageSize, List<Guid> exceedProductTypeIds)
        {
            var productTypes = await _repo.GetAllAsync(pageNumber, pageSize, exceedProductTypeIds);
            var productTypeResponse = _mapper.Map<List<ProductTypeResponse>>(productTypes.Items);
            var paginatedResult = new PaginatedList<ProductTypeResponse>(productTypeResponse, productTypes.TotalCount, productTypes.PageNumber, productTypes.PageSize);
            return Result<PaginatedList<ProductTypeResponse>>.Success(paginatedResult);
        }

        public async Task<Result<PaginatedList<ProductTypeResponse>>> GetAllByAdminAsync(int pageNumber, int pageSize, bool isActive)
        {
            var productTypes = await _repo.GetAllAdminAsync(pageNumber, pageSize, isActive);
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
                IsActive = productType.IsActive!.Value,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _repo.CreateAsync(newProductType);
            if (result == 0)
            {
                return Result.Failure("Nothing created", 400);
            }
            return Result.Success($"{newProductType.ProductTypeId}");
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
            var request = assignServicePackagesRequest.Requests;
            var servicePackageIds = request.Select(r => r.ServicePackageId).ToList();
            //check if duplicate packageId in request
            if (servicePackageIds.Count != servicePackageIds.Distinct().Count())
            {
                return Result.Failure($"Duplicate ServicePackageId in request", 400);
            }
            //check if productType exist
            var productType = await _repo.GetByIdAsync(productTypeId);
            if (productType == null)
            {
                return Result.Failure("productType not found", 404);
            }
            //check if all package exist
            var packageList = await _servicePackageRepository.GetByListIdAsync(servicePackageIds);
            var packageSet = packageList.Select(p => p.ServicePackageId).ToHashSet();
            if(servicePackageIds.Any(id => !packageSet.Contains(id)))
            {
                return Result.Failure($"Some ServicePackage not found or not activated", 404);
            }
            //check if mapping already exists with the same productTypeId and packageId
            var isMappingExist = await _servicePackageMappingRepository.CheckMappingExistAsync(productTypeId, servicePackageIds);
            if (isMappingExist)
            {
                return Result.Failure($"Mapping already exists for some ServicePackage", 400);
            }
            var requestDict = request.ToDictionary(r => r.ServicePackageId, r => r.Price);
            var packageDict = packageList.ToDictionary(p => p.ServicePackageId, p => p.Duration);
            //list lưu mapping id
            List<ServicePackageMapping> mappingList = new();
            foreach (var packageId in servicePackageIds)
            {
                
                ServicePackageMapping mapping = new ServicePackageMapping
                {
                    ProductTypeId = productTypeId,
                    ServicePackageId = packageId,
                    Duration = packageDict[packageId],
                    Price = requestDict[packageId],
                };
                mappingList.Add(mapping);
                _servicePackageMappingRepository.AddEntity(mapping);
            }
            var mappingIdList = mappingList.Select(m => m.ServicePackageMappingId).ToList();
            var result = await _unitOfWork.SaveChangeAsync();
            if(result == 0)
            {
                return Result.Failure($"Nothing created", 400);
            }
            return Result.Success(string.Join("\n",mappingIdList));
        }
        public async Task<Result> RemovePackagesAsync(Guid productTypeId, RemoveServicePackagesRequest removeServicePackagesRequest)
        {
            //check if duplicate packageId in request
            if (removeServicePackagesRequest.ServicePackageIds.Count != removeServicePackagesRequest.ServicePackageIds.Distinct().Count())
            {
                return Result.Failure($"Duplicate ServicePackageId in request", 400);
            }
            //check if productType exist
            var productType = await _repo.GetByIdAsync(productTypeId);
            if (productType == null)
            {
                return Result.Failure("productType not found", 404);
            }
            //check if mapping already exists with the same productTypeId and packageId
            var mappingList = await _servicePackageMappingRepository.GetListByIdAsync(productTypeId, removeServicePackagesRequest.ServicePackageIds);
            var mappingSet = mappingList.Select(m => m.ServicePackageId).ToHashSet();
            if(removeServicePackagesRequest.ServicePackageIds.Any(id => !mappingSet.Contains(id)))
            {
                 return Result.Failure($"Mapping not exists for some ServicePackage", 400);
            }
            _servicePackageMappingRepository.RemoveRange(mappingList);
            var result = await _unitOfWork.SaveChangeAsync();
            return Result.Success($"{result}");
        }
    }
}