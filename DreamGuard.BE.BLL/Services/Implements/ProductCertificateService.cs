using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ProductCertificateService : IProductCertificateService
    {
        private readonly IProductCertificateRepository _repository;
        private readonly IMapper _mapper;
        public ProductCertificateService(
            IProductCertificateRepository repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductCertificate>> CreateAsync(ProductCertificateCreateRequest request)
        {
            try
            {
                var entity = _mapper.Map<ProductCertificate>(request);
                await _repository.CreateAsync(entity);
                return Result<ProductCertificate>.Success(entity);
            }
            catch (Exception ex)
            {
                return Result<ProductCertificate>.Failure($"An error occurred while creating the product certificate: {ex.Message}", 500);
            }
        }

        public async Task<Result<PaginatedList<ProductCertificate>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var paginatedList = await _repository.GetAllAsync(pageNumber, pageSize);
            return Result<PaginatedList<ProductCertificate>>.Success(paginatedList);
        }

        public async Task<Result<ProductCertificate>> GetByIdAsync(Guid id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    return Result<ProductCertificate>.Failure("Product certificate not found", 400);
                }
                return Result<ProductCertificate>.Success(entity);
            }
            catch (Exception ex)
            {
                return Result<ProductCertificate>.Failure($"An error occurred while retrieving the product certificate: {ex.Message}", 500);
            }
        }

        public async Task<Result<List<ProductCertificate>>> GetByProductIdAsync(Guid productId)
        {
            var certificates = await _repository.GetByProductIdAsync(productId);
            return Result<List<ProductCertificate>>.Success(certificates);
        }

        public async Task<Result<ProductCertificate>> UpdateAsync(Guid id, ProductCertificateUpdateRequest request)
        {
            try
            {
                var existingEntity = await _repository.GetByIdAsync(id);
                if (existingEntity == null)
                {
                    return Result<ProductCertificate>.Failure("Product certificate not found", 400);
                }

                _mapper.Map(request, existingEntity);
                await _repository.UpdateAsync(existingEntity);
                return Result<ProductCertificate>.Success(existingEntity);
            }
            catch (Exception ex)
            {
                return Result<ProductCertificate>.Failure($"An error occurred while updating the product certificate: {ex.Message}", 500);
            }
        }

        public async Task<Result> UpdateStatusAsync(Guid id, bool isActive)
        {
            try
            {
                var existingEntity = await _repository.GetByIdAsync(id);
                if (existingEntity == null)
                {
                    return Result.Failure("Product certificate not found", 400);
                }

                existingEntity.IsActive = isActive;
                await _repository.UpdateAsync(existingEntity);
                return Result.Success("Product certificate status updated successfully");
            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occurred while updating the product certificate status: {ex.Message}", 500);
            }
        }
    }
}