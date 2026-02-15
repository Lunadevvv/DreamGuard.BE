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
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileService _UserProfileService;
        private readonly IMapper _mapper;
        public UserProfilesController(IUserProfileService UserProfileService, IMapper mapper)
        {
            _UserProfileService = UserProfileService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetByIdAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _UserProfileService.GetByIdAsync(userId);
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

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UserProfileUpdateRequest UserProfileRequest)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _UserProfileService.UpdateAsync(userId, UserProfileRequest);
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
        [HttpPost("ChangePhoneNumberRequest")]
        public async Task<IActionResult> ChangePhoneNumberRequestAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _UserProfileService.ChangePhoneNumberRequestAsync(userId);
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
        [HttpPost("ChangePhoneNumber")]
        public async Task<IActionResult> ChangePhoneNumberAsync([FromBody]ChangePhoneNumberRequest changePhoneNumberRequest)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _UserProfileService.ChangePhoneNumberAsync(userId, changePhoneNumberRequest);
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
