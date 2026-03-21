using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ProductCustomizeTypeService : IProductCustomizeTypeService
    {
        private readonly IProductCustomizeTypeRepository _productCustomizeTypeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductCustomizeTypeService(IProductCustomizeTypeRepository productCustomizeTypeRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _productCustomizeTypeRepository = productCustomizeTypeRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result> CreateProductCustomizeTypeAsync(ProductCustomizeType customizeType)
        {
            _productCustomizeTypeRepository.AddEntity(customizeType);
            var result = await _unitOfWork.SaveChangeAsync();

            if (result <= 0)
            {
                return Result.Failure("Failed to create product customize type.", 400);
            }
            return Result.Success($"Product customize type with name: {customizeType.Name} created successfully.");
        }

        public async Task<Result<ProductCustomizeType>> GetProductCustomizeTypeByIdAsync(Guid id)
        {
            var result = await _productCustomizeTypeRepository.GetByIdAsync(id);
            
            if (result == null)
            {
                return Result<ProductCustomizeType>.Failure("Product customize type not found.", 404);
            }

            return Result<ProductCustomizeType>.Success(result);
        }

        public async Task<Result<PaginatedList<ProductCustomizeType>>> GetProductCustomizeTypesAsync(int pageNumber, int pageSize)
        {
            var result = await _productCustomizeTypeRepository.GetAllWithPagingAsync(pageNumber, pageSize);

            return Result<PaginatedList<ProductCustomizeType>>.Success(result);
        }

        public async Task<Result> UpdateProductCustomizeTypeAsync(Guid id, ProductCustomizeType customizeType)
        {
            var existingType = await _productCustomizeTypeRepository.GetByIdAsync(id);
            if (existingType == null)
            {
                return Result.Failure("Product customize type not found.", 404);
            }

            _mapper.Map(customizeType, existingType);

            _productCustomizeTypeRepository.UpdateEntity(existingType);
            var result = await _unitOfWork.SaveChangeAsync();
            if (result <= 0)
            {
                return Result.Failure("Failed to update product customize type.", 400);
            }
            return Result.Success($"Product customize type with id: {id} updated successfully.");
        }
    }
}