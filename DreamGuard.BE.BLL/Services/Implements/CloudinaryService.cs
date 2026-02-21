using System.Reflection.Metadata;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly IProductAssetRepository _productAssetRepository;
        private readonly Cloudinary _cloudinary;
        private static string CLOUDINARY_FOLDER = "Product";
        private static long MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
        public CloudinaryService(IProductAssetRepository productAssetRepository, Cloudinary cloudinary)
        {
            _productAssetRepository = productAssetRepository;
            _cloudinary = cloudinary;
        }

        public async Task<Result<bool>> DeleteImageAsync(Guid assetId)
        {
            var asset = await _productAssetRepository.GetByIdAsync(assetId);
            if (asset == null)
            {
                return Result<bool>.Failure("Asset not found.", 404);
            }

            // Delete from Cloudinary
            var deletionParams = new DeletionParams(asset.PublicId);
            var deletionResult = _cloudinary.Destroy(deletionParams);
            if (deletionResult.Result != "ok")
            {
                return Result<bool>.Failure("Failed to delete image from Cloudinary.", 400);
            }

            // Delete from database
            var res = await _productAssetRepository.RemoveAsync(asset);
            if (!res)
            {
                return Result<bool>.Failure("Failed to delete image from database.", 400);
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<ProductAssetResponse>> UpdateImageAsync(Guid assetId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Result<ProductAssetResponse>.Failure("No file uploaded.", 400);
            }

            if(file.Length > MAX_FILE_SIZE)
            {
                return Result<ProductAssetResponse>.Failure("File size exceeds the maximum limit of 5MB.", 400);
            }

            // Get asset from database
            var asset = await _productAssetRepository.GetByIdAsync(assetId);
            if (asset == null)
            {
                return Result<ProductAssetResponse>.Failure("Asset not found.", 404);
            }

            //Overwrite file in Cloudinary
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = asset.PublicId,
                Overwrite = true,
                Invalidate = true, 
                Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
                // Folder = CLOUDINARY_FOLDER
            };

            //Get result from Cloudinary
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return Result<ProductAssetResponse>.Failure("Failed to upload image to Cloudinary.", 400);
            }

            //Update asset in database
            asset.Url = uploadResult.SecureUrl.ToString();
            var updateRes = await _productAssetRepository.UpdateAsync(asset);
            if (updateRes < 0)
            {
                return Result<ProductAssetResponse>.Failure("Failed to update image in database.", 400);
            }

            var response = new ProductAssetResponse
            {
                Id = asset.Id,
                Url = asset.Url,
                Type = asset.Type,
                PublicId = asset.PublicId,
                ProductId = asset.ProductId
            };

            return Result<ProductAssetResponse>.Success(response);
        }

        public async Task<Result<ProductAssetResponse>> UploadImageAsync(Guid productId, IFormFile file)
        {
            //Check if file is null or empty
            if (file == null || file.Length == 0)
            {
                return Result<ProductAssetResponse>.Failure("No file uploaded.", 400);
            }

            //Check if file size exceeds the limit
            if(file.Length > MAX_FILE_SIZE)
            {
                return Result<ProductAssetResponse>.Failure("File size exceeds the maximum limit of 5MB.", 400);
            }

            //Upload file to Cloudinary
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
                Folder = CLOUDINARY_FOLDER
            };

            //Get result from Cloudinary
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return Result<ProductAssetResponse>.Failure("Failed to upload image to Cloudinary.", 400);
            }

            //Save asset to database
            var asset = new ProductAsset
            {
                Url = uploadResult.SecureUrl.ToString(),
                Type = file.ContentType,
                PublicId = uploadResult.PublicId,
                ProductId = productId
            };

            var res = await _productAssetRepository.CreateAsync(asset);
            if (res < 0)
            {
                return Result<ProductAssetResponse>.Failure("Failed to save image to database.", 400);
            }

            var response = new ProductAssetResponse
            {
                Id = asset.Id,
                Url = asset.Url,
                Type = asset.Type,
                PublicId = asset.PublicId,
                ProductId = asset.ProductId
            };

            return Result<ProductAssetResponse>.Success(response);
        }
    }
}