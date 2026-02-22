using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Http;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<Result<ProductAssetResponse>> UploadImageAsync(Guid productId, IFormFile file);
        Task<Result<ProductAssetResponse>> UploadImageWithoutSaveDbAsync(IFormFile file);
        Task<Result<ProductAssetResponse>> UpdateImageAsync(Guid assetId, IFormFile file);
        Task<Result<bool>> DeleteImageAsync(Guid assetId);
    }
}