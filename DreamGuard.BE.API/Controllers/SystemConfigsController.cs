using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DreamGuard.BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Role.Admin)]
    public class SystemConfigsController : ControllerBase
    {
        private readonly ISystemConfigService _configService;

        public SystemConfigsController(ISystemConfigService configService)
        {
            _configService = configService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllConfigs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _configService.GetAllConfigsAsync(pageNumber, pageSize);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }
            return Ok(result.Data);
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> GetConfigByKey(string key)
        {
            var result = await _configService.GetConfigByKeyAsync(key);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateConfig([FromBody] SystemConfigCreateRequest request)
        {
            var result = await _configService.CreateConfigAsync(request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }
            return Ok(result.Message);
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> UpdateConfig(string key, [FromBody] SystemConfigUpdateRequest request)
        {
            var result = await _configService.UpdateConfigAsync(key, request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }
            return Ok(result.Message);
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> DeleteConfig(string key)
        {
            var result = await _configService.DeleteConfigAsync(key);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }
            return Ok(result.Message);
        }
    }
}
