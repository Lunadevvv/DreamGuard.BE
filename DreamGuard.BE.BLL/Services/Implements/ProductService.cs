using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using DreamGuard.BE.BLL.Requests;
using AutoMapper;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductVariantService _variantService;
        private readonly IProductCustomizeTypeRepository _customizeTypeRepository;
        private readonly IProductCertificateRepository _productCertificateRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ProductService(
            IProductRepository productRepository, 
            IProductVariantRepository variantRepository, 
            IProductVariantService variantService, 
            IUnitOfWork unitOfWork,
            IProductCustomizeTypeRepository customizeTypeRepository,
            IProductCertificateRepository productCertificateRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _variantRepository = variantRepository;
            _variantService = variantService;
            _unitOfWork = unitOfWork;
            _customizeTypeRepository = customizeTypeRepository;
            _productCertificateRepository = productCertificateRepository;
            _mapper = mapper;
        }

        public async Task<Result<bool>> CreateProductAsync(CreateProductRequest product)
        {
            //Check if product with the same slug already exists
            var existingProduct = await _productRepository.GetProductBySlugAsync(product.Slug);
            if (existingProduct != null)
            {
                return Result<bool>.Failure("A product with the same slug already exists.", 400);
            }

            //get certificates by ids
            var certificates = await _productCertificateRepository.GetByProductCertificateIdsAsync(product.CertificateIds);

            //check if any certificate id is invalid
            if(certificates.Count != product.CertificateIds.Count)
            {
                return Result<bool>.Failure("One or more certificate ids are invalid.", 400);
            }

           //map request to product
            var newProduct = new Product
            {
                Name = product.Name,
                Summary = product.Summary,
                Description = product.Description,
                Material = product.Material,
                AgeGroup = product.AgeGroup,
                WarrantyPolicyDay = product.WarrantyPolicyDay,
                ReturnPolicyDay = product.ReturnPolicyDay,
                Slug = product.Slug,
                CateId = product.CateId,
                IsTradeInEligible = product.IsTradeInEligible,
                MinTradeInPrice = product.MinTradeInPrice,
                DepositAmount = product.DepositAmount,
            };

            //add product to database
            _productRepository.AddEntity(newProduct);
            await _productRepository.UpdateProductCertificatesAsync(newProduct, certificates);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> UpdateProductAsync(UpdateProductRequest product)
        {
            //Check if product exists
            var existingProduct = await _productRepository.GetProductByIdForUpdateAsync(product.Id);
            if (existingProduct == null)
            {
                return Result<bool>.Failure("Product not found.", 404);
            }

            //Check if product with the same slug already exists (exclude current product)
            var productWithSameSlug = await _productRepository.GetProductBySlugAsync(product.Slug);
            if (productWithSameSlug != null && productWithSameSlug.Id != product.Id)
            {
                return Result<bool>.Failure("A product with the same slug already exists.", 400);
            }

            //get certificates by ids
            var certificates = await _productCertificateRepository.GetByProductCertificateIdsAsync(product.CertificateIds);

            //check if any certificate id is invalid
            if(certificates.Count != product.CertificateIds.Count)
            {
                return Result<bool>.Failure("One or more certificate ids are invalid.", 400);
            }

            // Manually map fields instead of using AutoMapper to prevent tracking issues with Navigation Properties
            existingProduct.Name = product.Name;
            existingProduct.Summary = product.Summary;
            existingProduct.Slug = product.Slug;
            existingProduct.Description = product.Description;
            existingProduct.Material = product.Material;
            existingProduct.AgeGroup = product.AgeGroup;
            existingProduct.WarrantyPolicyDay = product.WarrantyPolicyDay;
            existingProduct.ReturnPolicyDay = product.ReturnPolicyDay;
            existingProduct.FullyCustomizedProductType = product.FullyCustomizedProductType;
            existingProduct.CateId = product.CateId;
            existingProduct.IsTradeInEligible = product.IsTradeInEligible;
            existingProduct.MinTradeInPrice = product.MinTradeInPrice;
            existingProduct.DepositAmount = product.DepositAmount;

            await _productRepository.UpdateProductCertificatesAsync(existingProduct, certificates);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> UpdateProductStatusAsync(Guid id, ProductStatus status)
        {
            var prod = await _productRepository.GetProductByIdAsync(id);
            if (prod == null)
            {
                return Result<bool>.Failure("Product not found.", 404);
            }

            prod.Status = status;
            var res = await _productRepository.UpdateAsync(prod);
            if (res < 0)
            {
                return Result<bool>.Failure("Failed to update product status.", 400);
            }

            // Cascade: when product is Hidden, also hide all its variants
            if (status == ProductStatus.Hidden)
            {
                var variants = await _variantRepository.GetVariantsByProductIdForStockCheckAsync(id);
                foreach (var variant in variants)
                {
                    variant.Status = ProductStatus.Hidden;
                }
                await _unitOfWork.SaveChangeAsync();
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedList<ProductResponse>>> GetAllProductByCategoryAsync(int cateId, int pageNumber, decimal? maxPrice, string? color, int? maxAgeGroup)
        {
            var products = await _productRepository.GetAllProductByCategoryAsync(cateId, pageNumber, maxPrice, color, maxAgeGroup);

            //check if products is null or empty
            if (products == null || !products.Items.Any())
            {
                return Result<PaginatedList<ProductResponse>>.Failure("No products found for the given category.", 404);
            }

            // Map products to ProductResponse
            var productResponses = products.Items.Select(p =>
            {
                var hasVariants = p.Variants != null && p.Variants.Any();
                return new ProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Summary = p.Summary,
                    Slug = p.Slug,
                    Material = p.Material,
                    AgeGroup = p.AgeGroup,
                    AverageRating = p.AverageRating,
                    BasePrice = hasVariants ? p.Variants.Min(v => v.BasePrice) : 0,
                    SalePrice = hasVariants ? p.Variants.Min(v => v.SalePrice) : 0,
                    IsTradeInEligible = p.IsTradeInEligible,
                    MinTradeInPrice = p.MinTradeInPrice,
                    DepositAmount = p.DepositAmount,
                    ImageUrls = p.Assets.Select(a => a.Url).ToList()
                };
            }).ToList();

            // Create a new PaginatedList for ProductResponse
            var paginatedResponse = new PaginatedList<ProductResponse>(
                productResponses,
                products.TotalCount,
                products.PageNumber,
                products.PageSize
            );

            return Result<PaginatedList<ProductResponse>>.Success(paginatedResponse);
        }
        public async Task<Result<PaginatedList<ProductResponse>>> GetAllProductToTradeInAsync(int? cateId, int pageNumber, int pageSize, decimal? maxPrice, string? color, int? maxAgeGroup)
        {
            var products = await _productRepository.GetAllProductToTradeInAsync(cateId, pageNumber, pageSize, maxPrice, color, maxAgeGroup);

            // Map products to ProductResponse
            var productResponses = products.Items.Select(p =>
            {
                var hasVariants = p.Variants != null && p.Variants.Any();
                return new ProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Summary = p.Summary,
                    Slug = p.Slug,
                    Material = p.Material,
                    AgeGroup = p.AgeGroup,
                    AverageRating = p.AverageRating,
                    BasePrice = hasVariants ? p.Variants.Min(v => v.BasePrice) : 0,
                    SalePrice = hasVariants ? p.Variants.Min(v => v.SalePrice) : 0,
                    IsTradeInEligible = p.IsTradeInEligible,
                    MinTradeInPrice = p.MinTradeInPrice,
                    DepositAmount = p.DepositAmount,
                    ImageUrls = p.Assets.Select(a => a.Url).ToList()
                };
            }).ToList();

            // Create a new PaginatedList for ProductResponse
            var paginatedResponse = new PaginatedList<ProductResponse>(
                productResponses,
                products.TotalCount,
                products.PageNumber,
                products.PageSize
            );

            return Result<PaginatedList<ProductResponse>>.Success(paginatedResponse);
        }


        public async Task<Result<Product>> GetProductByIdAsync(Guid id)
        {
            var prod = await _productRepository.GetProductByIdAsync(id);

            //check if product is null
            if (prod == null)
            {
                return Result<Product>.Failure("Product not found.", 404);
            }

            return Result<Product>.Success(prod);
        }

        public async Task<Result<ProductDetailResponse>> GetProductDetailBySlugAsync(string slug)
        {
            var prod = await _productRepository.GetProductBySlugAsync(slug);

            //check if product is null
            if (prod == null)
            {
                return Result<ProductDetailResponse>.Failure("Product not found.", 404);
            }

            var variantsResult = await _variantService.GetVariantsByProductIdAsync(prod.Id, null, null);
            if (!variantsResult.Succeeded)
            {
                return Result<ProductDetailResponse>.Failure("Failed to retrieve product variants.", variantsResult.StatusCode);
            }

            // Map product to ProductDetailResponse
            var productDetailResponse = new ProductDetailResponse
            {
                Id = prod.Id,
                Name = prod.Name,
                Summary = prod.Summary,
                Description = prod.Description,
                Material = prod.Material,
                AgeGroup = prod.AgeGroup,
                AverageRating = prod.AverageRating,
                WarrantyPolicyDay = prod.WarrantyPolicyDay,
                ReturnPolicyDay = prod.ReturnPolicyDay,
                Status = prod.Status,
                Variants = variantsResult.Data!,
                FullyCustomizedProductType = prod.FullyCustomizedProductType,
                ImageUrls = prod.Assets.Select(a => a.Url).ToList()
            };

            return Result<ProductDetailResponse>.Success(productDetailResponse);
        }

        public async Task<Result<PaginatedList<ProductResponseForAdmin>>> GetAllProductsForAdminAsync(int pageNumber, string? name)
        {
            var products = await _productRepository.GetAllProductsForAdminAsync(pageNumber, name);

            //check if products is null or empty
            if (products == null || !products.Items.Any())
            {
                return Result<PaginatedList<ProductResponseForAdmin>>.Failure("No products found for the given category.", 404);
            }

            // Map products to ProductResponseForAdmin
            var productResponses = products.Items.Select(p =>
            {
                var hasVariants = p.Variants != null && p.Variants.Count > 0;
                return new ProductResponseForAdmin
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    Material = p.Material,
                    AgeGroup = p.AgeGroup,
                    AverageRating = p.AverageRating,
                    MinPrice = hasVariants ? p.Variants.Min(v => v.BasePrice) : 0,
                    MaxPrice = hasVariants ? p.Variants.Max(v => v.BasePrice) : 0,
                    Status = p.Status,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                    VariantCount = p.Variants.Count,
                    DepositAmount = p.DepositAmount,
                    IsTradeInEligible = p.IsTradeInEligible,
                    MinTradeInPrice = p.MinTradeInPrice,
                    FullyCustomizedProductType = p.FullyCustomizedProductType
                };
            }).ToList();

            // Create a new PaginatedList for ProductResponse
            var paginatedResponse = new PaginatedList<ProductResponseForAdmin>(
                productResponses,
                products.TotalCount,
                products.PageNumber,
                products.PageSize
            );

            return Result<PaginatedList<ProductResponseForAdmin>>.Success(paginatedResponse);
        }

        public async Task<Result<ProductDetailResponse>> CreateFullyCustomizeProductAsync(CreateFullyCustomizeProductRequest request)
        {
            //check if product with the same slug already exists
            var existingProduct = await _productRepository.GetProductBySlugAsync(request.Slug);
            if (existingProduct != null)
            {
                return Result<ProductDetailResponse>.Failure("A product with the same slug already exists.", 400);
            }

            //get all customize type ids valid for this specific product type
            var allCustomizeTypeIds = await _customizeTypeRepository.GetCustomizeTypeIdsByProductTypeAsync(request.FullyCustomizedProductType);

            //map request to product
            var product = new Product
            {
                Name = request.Name,
                Summary = request.Summary,
                Description = request.Description,
                Material = request.Material,
                AgeGroup = request.AgeGroup,
                WarrantyPolicyDay = request.WarrantyPolicyDay,
                ReturnPolicyDay = request.ReturnPolicyDay,
                Slug = request.Slug,
                FullyCustomizedProductType = request.FullyCustomizedProductType,
                IsTradeInEligible = request.IsTradeInEligible,
                MinTradeInPrice = request.MinTradeInPrice,
                DepositAmount = request.DepositAmount
            };

            //begin transaction
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                //create product
                _productRepository.AddEntity(product);
                await _unitOfWork.SaveChangeAsync();

                //map request to CreateVariantWithCustomizeRequest
                var createVariantRequest = new CreateVariantWithCustomizeRequest
                {
                    Sku = request.Sku,
                    ProductId = product.Id,
                    BasePrice = request.BasePrice,
                    SalePrice = request.SalePrice,
                    Weight = request.Weight,
                    CustomizeTypeIds = allCustomizeTypeIds
                };

                //create variant with customize types
                var variantResult = await _variantService.CreateVariantWithFullyCustomizeAsync(createVariantRequest);
                if (!variantResult.Succeeded)
                {
                    //rollback transaction
                    await transaction.RollbackAsync();
                    return Result<ProductDetailResponse>.Failure("Failed to create product variant. " + variantResult.Error, variantResult.StatusCode);
                }

                //commit transaction
                await transaction.CommitAsync();

                //get product detail
                var productDetailResult = await GetProductDetailBySlugAsync(product.Slug);
                if (!productDetailResult.Succeeded)
                {
                    return Result<ProductDetailResponse>.Failure("Product created but failed to retrieve product detail. " + productDetailResult.Error, productDetailResult.StatusCode);
                }

                return Result<ProductDetailResponse>.Success(productDetailResult.Data!);

            }
            catch (Exception ex)
            {
                //rollback transaction
                await transaction.RollbackAsync();
                return Result<ProductDetailResponse>.Failure("Failed to create fully customized product. " + ex.Message, 400);
            }
        }

        public async Task<Result<List<ProductResponse>>> GetFullyCustomizedProductsAsync()
        {
            var products = await _productRepository.GetFullyCustomizedProductsAsync();

            //check if products is null or empty
            if (products == null || !products.Any())
            {
                return Result<List<ProductResponse>>.Failure("No products found.", 404);
            }

            // Map products to ProductResponse
            var productResponses = products.Select(p =>
            {
                var hasVariants = p.Variants != null && p.Variants.Any();
                return new ProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Summary = p.Summary,
                    Slug = p.Slug,
                    Material = p.Material,
                    AgeGroup = p.AgeGroup,
                    AverageRating = p.AverageRating,
                    BasePrice = hasVariants ? p.Variants.Min(v => v.BasePrice) : 0,
                    SalePrice = hasVariants ? p.Variants.Min(v => v.SalePrice) : 0,
                    IsTradeInEligible = p.IsTradeInEligible,
                    MinTradeInPrice = p.MinTradeInPrice,
                    DepositAmount = p.DepositAmount,
                    ImageUrls = p.Assets.Select(a => a.Url).ToList()
                };
            }).ToList();

            return Result<List<ProductResponse>>.Success(productResponses);
        }
    }
}