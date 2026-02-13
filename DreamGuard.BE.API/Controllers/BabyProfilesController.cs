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
    public class BabyProfilesController : ControllerBase
    {
        private readonly IBabyProfileService _babyProfileService;
        private readonly IMapper _mapper;
        public BabyProfilesController(IBabyProfileService babyProfileService, IMapper mapper)
        {
            _babyProfileService = babyProfileService;
            _mapper = mapper;
        }
        [HttpGet]
     
        public async Task<IActionResult> GetAllAsync(int pageNumber = 1)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _babyProfileService.GetAllAsync(userId, pageNumber);
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
        [HttpGet("{babyId}")]
        public async Task<IActionResult> GetByIdAsync(Guid babyId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _babyProfileService.GetByIdAsync(userId, babyId);
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
        public async Task<IActionResult> CreateAsync([FromBody] BabyProfileCreateRequest babyProfileRequest)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var babyProfile = _mapper.Map<BabyProfile>(babyProfileRequest);
            babyProfile.UserId = userId;
            var result = await _babyProfileService.CreateAsync(babyProfile);
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
		[HttpPut("{babyId}")]
		public async Task<IActionResult> UpdateAsync(Guid babyId, [FromBody] BabyProfileUpdateRequest babyProfileRequest)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _babyProfileService.UpdateAsync(userId, babyId, babyProfileRequest);
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
        [HttpDelete("{babyId}")]
        public async Task<IActionResult> RemoveAsync(Guid babyId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _babyProfileService.RemoveAsync(userId, babyId);
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
