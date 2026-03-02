using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<Result> CreateCategoryAsync(Category request)
        {
            var result = await _categoryRepository.CreateAsync(request);
            if (result > 0)
            {
                return Result.Success("Category created successfully.");
            }
            else
            {
                return Result.Failure("Failed to create category.", 500);
            }
        }

        public async Task<Result<List<CategoryResponse>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllCategoriesAsync();
                var categoryResponses = categories.Select(c => new CategoryResponse
                {
                    CateId = c.CateId,
                    Name = c.Name,
                    IsActive = c.IsActive,
                    slug = c.slug,
                    ChildCategoryList = c.ChildCategoryList.Select(child => new CategoryResponse
                    {
                        CateId = child.CateId,
                        Name = child.Name,
                        IsActive = child.IsActive,
                        slug = child.slug
                    }).ToList()
                }).ToList();

                return Result<List<CategoryResponse>>.Success(categoryResponses);
            }
            catch (Exception ex)
            {
                return Result<List<CategoryResponse>>.Failure("An error occurred while retrieving categories.", 500);
            }
        }

        public async Task<Result> UpdateCategoryAsync(int id, Category request)
        {
            //get category by id
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return Result.Failure("Category not found.", 404);
            }

            //map request to category
            category.Name = request.Name;
            category.IsActive = request.IsActive;
            category.slug = request.slug;
            category.CateParentId = request.CateParentId;
            
            //update category and save changes
            var result = await _categoryRepository.UpdateAsync(category);
            if (result > 0)
            {
                return Result.Success("Category updated successfully.");
            }
            else
            {
                return Result.Failure("Failed to update category.", 500);
            }
        }
    }
}