using AutoMapper;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Requests;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly IMapper _mapper;
        public AddressesController(IAddressService AddressService, IMapper mapper)
        {
            _addressService = AddressService;
            _mapper = mapper;
        }
        [HttpGet]
     
        public async Task<IActionResult> GetAllAsync(int pageNumber = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _addressService.GetAllAsync(userId, pageNumber);
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
        [HttpGet("{addressId}")]
        public async Task<IActionResult> GetByIdAsync(string addressId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _addressService.GetByIdAsync(userId, addressId);
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
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] AddressCreateRequest AddressRequest)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var Address = _mapper.Map<Address>(AddressRequest);
            Address.UserId = userId;
            var result = await _addressService.CreateAsync(Address);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result);
        }
		[HttpPut("{addressId}")]
		public async Task<IActionResult> UpdateAsync(string addressId, [FromBody] AddressUpdateRequest AddressRequest)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await _addressService.UpdateAsync(userId, addressId, AddressRequest);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result);
        }
        [HttpDelete("{addressId}")]
        public async Task<IActionResult> RemoveAsync(string addressId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _addressService.RemoveAsync(userId, addressId);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result);
        }
    }
}
