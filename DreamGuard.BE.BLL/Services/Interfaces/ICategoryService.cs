using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Result> CreateCategoryAsync(Category request);
        Task<Result<List<CategoryResponse>>> GetAllCategoriesAsync();
        Task <Result> UpdateCategoryAsync(int id, Category request);
    }
}