using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class SystemConfigService : ISystemConfigService
    {
        private readonly ISystemConfigRepository _configRepository;
        private readonly IMapper _mapper;

        public SystemConfigService(ISystemConfigRepository configRepository, IMapper mapper)
        {
            _configRepository = configRepository;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<SystemConfigResponse>>> GetAllConfigsAsync(int pageNumber, int pageSize)
        {
            var data = await _configRepository.GetAllConfigsAsync(pageNumber, pageSize);
            var responses = _mapper.Map<List<SystemConfigResponse>>(data.Items);
            var paginatedList = new PaginatedList<SystemConfigResponse>(responses, data.TotalCount, pageNumber, pageSize);
            return Result<PaginatedList<SystemConfigResponse>>.Success(paginatedList);
        }

        public async Task<Result<SystemConfigResponse>> GetConfigByKeyAsync(string key)
        {
            var config = await _configRepository.GetByKeyAsync(key);
            if (config == null) return Result<SystemConfigResponse>.Failure("Config not found", 404);
            return Result<SystemConfigResponse>.Success(_mapper.Map<SystemConfigResponse>(config));
        }

        public async Task<Result> CreateConfigAsync(SystemConfigCreateRequest request)
        {
            var existingConfig = await _configRepository.GetByKeyAsync(request.ConfigKey);
            if (existingConfig != null) return Result.Failure("Config with this key already exists", 400);

            var config = _mapper.Map<SystemConfig>(request);
            await _configRepository.CreateAsync(config);
            return Result.Success("Config created successfully");
        }

        public async Task<Result> UpdateConfigAsync(string key, SystemConfigUpdateRequest request)
        {
            var config = await _configRepository.GetByKeyAsync(key);
            if (config == null) return Result.Failure("Config not found", 404);

            _mapper.Map(request, config);
            config.UpdatedAt = DateTime.UtcNow;
            await _configRepository.UpdateAsync(config);
            return Result.Success("Config updated successfully");
        }

        public async Task<Result> DeleteConfigAsync(string key)
        {
            var config = await _configRepository.GetByKeyAsync(key);
            if (config == null) return Result.Failure("Config not found", 404);

            await _configRepository.RemoveAsync(config);
            return Result.Success("Config deleted successfully");
        }
    }
}
