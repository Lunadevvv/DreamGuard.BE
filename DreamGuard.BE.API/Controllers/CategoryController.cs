using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Implements;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;
        private readonly IMapper _mapper;
        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger, IMapper mapper)
        {
            _categoryService = categoryService;
            _logger = logger;
            _mapper = mapper;
        }

        //Get all categories
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories.Data);
        }

        //Create new category
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] Category request)
        {
            var result = await _categoryService.CreateCategoryAsync(request);
            if(!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Message);
            }
            return Ok(result);
        }

        //Update category
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category request)
        {
            var result = await _categoryService.UpdateCategoryAsync(id, request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Message);
            }
            return Ok(result);
        }
    }
}