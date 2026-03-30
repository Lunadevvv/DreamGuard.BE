using System;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamGuard.BE.API.Controllers
{
    [ApiController]
    [Route("api/certificate")]
    [Authorize(Roles = "Admin")]
    public class ProductCertificateController : ControllerBase
    {
        private readonly IProductCertificateService _productCertificateService;
        public ProductCertificateController(IProductCertificateService productCertificateService)
        {
            _productCertificateService = productCertificateService;
        }

        //Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCertificateCreateRequest request)
        {
            var result = await _productCertificateService.CreateAsync(request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result.Data);
        }

        //Update
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProductCertificateUpdateRequest request)
        {
            var result = await _productCertificateService.UpdateAsync(id, request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result.Data);
        }

        //Update Status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] bool isActive)
        {
            var result = await _productCertificateService.UpdateStatusAsync(id, isActive);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok($"Product certificate with id {id} has been {(isActive ? "activated" : "deactivated")} successfully.");
        }

        //Get all with pagination
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _productCertificateService.GetAllAsync(pageNumber, pageSize);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result.Data);
        }

        //get by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _productCertificateService.GetByIdAsync(id);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result.Data);
        }

        //get by product id
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            var result = await _productCertificateService.GetByProductIdAsync(productId);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result.Data); 
        }
    }
}