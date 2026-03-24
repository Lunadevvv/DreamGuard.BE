using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ServiceEvidenceService : IServiceEvidenceService
    {
        private readonly IServiceTaskRepository _serviceTaskRepository;
        private readonly IServiceEvidenceRepository _serviceEvidenceRepository;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IUnitOfWork _unitOfWork;
        public ServiceEvidenceService(IServiceTaskRepository serviceTaskRepository, IServiceEvidenceRepository serviceEvidenceRepository, IMapper mapper, ICloudinaryService cloudinaryService, IUnitOfWork unitOfWork)
        {
            _serviceTaskRepository = serviceTaskRepository;
            _serviceEvidenceRepository = serviceEvidenceRepository;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PaginatedList<ServiceEvidenceResponse>>> AdminSearchSeAsync(int pageNumber, int pageSize, AdminSearchSeRequest adminSearchSeRequest)
        {
            var serviceEvidences = await _serviceEvidenceRepository.AdminSearchSeAsync(adminSearchSeRequest.PageNumber, adminSearchSeRequest.PageSize, adminSearchSeRequest.ServiceEvidenceId, adminSearchSeRequest.ServiceTaskId);
            var serviceEvidenceResponse = _mapper.Map<List<ServiceEvidenceResponse>>(serviceEvidences.Items);
            var paginatedResult = new PaginatedList<ServiceEvidenceResponse>(serviceEvidenceResponse, serviceEvidences.TotalCount, serviceEvidences.PageNumber, serviceEvidences.PageSize);
            return Result<PaginatedList<ServiceEvidenceResponse>>.Success(paginatedResult);
        }

        public async Task<Result> CreateAsync(Guid staffId, ServiceEvidenceCreateRequest createRequest)
        {
            var serviceTask = await _serviceTaskRepository.GetByIdAsync(createRequest.ServiceTaskId);
            if (serviceTask == null)
            {
                return Result.Failure("Service task not found.", 404);
            }
            if (serviceTask.StaffId != staffId)
            {
                return Result.Failure("This is not your staff task", 401);
            }
            if (serviceTask.Status != ServiceTaskStatus.CheckedOut)
            {
                return Result.Failure("Service task is not in CheckedOut.", 400);
            }
            List<ServiceEvidence> evidenceList = new();
            foreach (var file in createRequest.Files)
            {
                var uploadImageResult = await _cloudinaryService.UploadImageAsync(file, "SERVICE_EVIDENCE_FOLDER");
                if (!uploadImageResult.Succeeded)
                {
                    return Result.Failure($"Failed to upload image: {uploadImageResult.Error}", 500);
                }
                var serviceEvidence = new ServiceEvidence
                {
                    ServiceTaskId = createRequest.ServiceTaskId,
                    Description = createRequest.Description,
                    ImageUrl = uploadImageResult.Data.Url,
                    PublicId = uploadImageResult.Data.PublicId,
                    EvidenceType = file.ContentType
                };
                evidenceList.Add(serviceEvidence);
                _serviceEvidenceRepository.AddEntity(serviceEvidence);
            }
            var evidenceIds = evidenceList.Select(e => e.SeId).ToList();
            var result = await _unitOfWork.SaveChangeAsync();
            if (result == 0)
            {
                return Result.Failure("Nothing created", 400);
            }
            return Result.Success(string.Join("\n", evidenceIds));
        }

        public async Task<Result<PaginatedList<ServiceEvidenceResponse>>> GetAllAsync(Guid staffId, int pagenumber, int pageSize, Guid serviceTaskId)
        {
            var serviceTask = await _serviceTaskRepository.GetByIdAsync(serviceTaskId);
            if (serviceTask == null)
            {
                return Result<PaginatedList<ServiceEvidenceResponse>>.Failure("Service task not found.", 404);
            }
            if (serviceTask.StaffId != staffId)
            {
                return Result<PaginatedList<ServiceEvidenceResponse>>.Failure("This is not your staff task", 403);
            }
            var serviceEvidences = await _serviceEvidenceRepository.GetAllAsync(pagenumber, pageSize, serviceTaskId);
            if (serviceEvidences == null || serviceEvidences.TotalCount == 0 )
            {
                return Result<PaginatedList<ServiceEvidenceResponse>>.Failure("Service evidences not found.", 404);
            }
            var serviceEvidenceResponse = _mapper.Map<List<ServiceEvidenceResponse>>(serviceEvidences.Items);
            var paginatedResult = new PaginatedList<ServiceEvidenceResponse>(serviceEvidenceResponse, serviceEvidences.TotalCount, serviceEvidences.PageNumber, serviceEvidences.PageSize);
            return Result<PaginatedList<ServiceEvidenceResponse>>.Success(paginatedResult);
        }

        public async Task<Result<ServiceEvidenceResponse>> GetByIdAsync(Guid serviceEvidenceId)
        {
            var result = await _serviceEvidenceRepository.GetByIdAsync(serviceEvidenceId);
            if (result == null)
            {
                return Result<ServiceEvidenceResponse>.Failure("service evidence not found.", 404);
            }
            var serviceEvidenceResponse = _mapper.Map<ServiceEvidence, ServiceEvidenceResponse>(result);
            return Result<ServiceEvidenceResponse>.Success(serviceEvidenceResponse);
        }
    }
}
