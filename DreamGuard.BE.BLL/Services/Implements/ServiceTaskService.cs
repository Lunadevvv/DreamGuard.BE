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
using System.Security.Cryptography.X509Certificates;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ServiceTaskService : IServiceTaskService
    {
        private readonly IServiceTaskRepository _repo;
        private readonly IMapper _mapper;
        private readonly IServiceOrderRepository _soRepo;
        private readonly IStaffRepository _staffRepo;
        private readonly IUnitOfWork _unitOfWork;
        IPaymentRepository _paymentRepo;

        public ServiceTaskService(IServiceTaskRepository repo, IMapper mapper, IServiceOrderRepository soRepo, IStaffRepository staffRepo, IUnitOfWork unitOfWork, IPaymentRepository paymentRepo)
        {
            _repo = repo;
            _mapper = mapper;
            _soRepo = soRepo;
            _staffRepo = staffRepo;
            _unitOfWork = unitOfWork;
            _paymentRepo = paymentRepo;
        }

        public async Task<Result> CreateAsync(ServiceTaskCreateRequest serviceTaskCreateRequest)
        {
            var serviceOrder = await _soRepo.GetByIdWithServiceTask(serviceTaskCreateRequest.SoId);
            if (serviceOrder == null)
            {
                return Result.Failure("Service order not found.", 404);
            }
            if(serviceOrder.ServiceTask != null)
            {
                return Result.Failure("Service task already exists for this service order.", 400);
            }
            var staff = await _staffRepo.GetByIdAsync(serviceTaskCreateRequest.StaffId);
            if (staff == null)
            {
                return Result.Failure("Staff not found.", 404);
            }
            if (serviceOrder.Status != OrderServiceStatus.Confirmed)
            {
                return Result.Failure("Service order must be confirmed before assigning service task", 400);
            }
            var serviceTask = new ServiceTask
            {
                SoId = serviceTaskCreateRequest.SoId,
                StaffId = serviceTaskCreateRequest.StaffId,
            };
            var result = await _repo.CreateAsync(serviceTask);
            if (result == 0)
            {
                return Result.Failure("Nothing created", 400);
            }

            return Result.Success($"{serviceTask.ServiceTaskId}");
        }


        public async Task<Result<PaginatedList<ServiceTaskResponse>>> GetBySoIdAsync(Guid soId, int pageNumber, int pageSize)
        {
            var serviceOrder = await _soRepo.GetByIdAsync(soId);
            if (serviceOrder == null)
            {
                return Result<PaginatedList<ServiceTaskResponse>>.Failure("Service order not found.", 404);
            }
            var serviceTasks = await _repo.GetBySoIdAsync(soId, pageNumber, pageSize);
            if (serviceTasks == null || serviceTasks.TotalCount == 0)
            {
                return Result<PaginatedList<ServiceTaskResponse>>.Failure("No service tasks found for this service order.", 404);
            }
            var serviceTaskResponses = _mapper.Map<List<ServiceTaskResponse>>(serviceTasks.Items);
            var paginatedResult = new PaginatedList<ServiceTaskResponse>(serviceTaskResponses, serviceTasks.TotalCount, serviceTasks.PageNumber, serviceTasks.PageSize);
            return Result<PaginatedList<ServiceTaskResponse>>.Success(paginatedResult);
        }

        public async Task<Result<PaginatedList<ServiceTaskResponse>>> SearchAsync(AdminSearchServiceTaskRequest searchRequest, int pageNumber, int pageSize)
        {
            var serviceTasks = await _repo.SearchAsync(searchRequest.StaffId, searchRequest.SoId, pageNumber, pageSize);
            var serviceTaskResponses = _mapper.Map<List<ServiceTaskResponse>>(serviceTasks.Items);
            var paginatedResult = new PaginatedList<ServiceTaskResponse>(serviceTaskResponses, serviceTasks.TotalCount, serviceTasks.PageNumber, serviceTasks.PageSize);
            return Result<PaginatedList<ServiceTaskResponse>>.Success(paginatedResult);
        }
        public async Task<Result<PaginatedList<ServiceTaskResponse>>> GetByStaffIdAsync(Guid staffId, int pageNumber, int pageSize)
        {
            var serviceTasks = await _repo.GetByStaffIdAsync(staffId, pageNumber, pageSize);
            var serviceTaskResponses = _mapper.Map<List<ServiceTaskResponse>>(serviceTasks.Items);
            var paginatedResult = new PaginatedList<ServiceTaskResponse>(serviceTaskResponses, serviceTasks.TotalCount, serviceTasks.PageNumber, serviceTasks.PageSize);
            return Result<PaginatedList<ServiceTaskResponse>>.Success(paginatedResult);
        }

        public async Task<Result<ServiceTaskDetailResponse>> GetByIdAsync(Guid serviceTaskId, Guid staffId, string role)
        {
            var serviceTask = await _repo.GetByIdWithDetailsAsync(serviceTaskId);
            if (serviceTask == null)
            {
                return Result<ServiceTaskDetailResponse>.Failure("Service task not found.", 404);
            }
            if (role != Role.Admin && role != Role.Manager &&  serviceTask.StaffId != staffId)
            {
                return Result<ServiceTaskDetailResponse>.Failure("You are not assigned to this service task.", 403);
            }
            var serviceTaskDetailResponse = new ServiceTaskDetailResponse()
            {
                SoId = serviceTask.SoId,
                StaffId = serviceTask.StaffId,
                ServiceTaskId = serviceTask.ServiceTaskId,
                Status = serviceTask.Status,
                CheckIn = serviceTask.CheckIn,
                CheckOut = serviceTask.CheckOut,
                StaffNote = serviceTask.StaffNote,
                ServiceOrderStatus = serviceTask.ServiceOrder.Status,
                CustomerNote = serviceTask.ServiceOrder.CustomerNote,
                ReceiverName = serviceTask.ServiceOrder.ReceiverName,
                Address = serviceTask.ServiceOrder.Address,
                TotalPrice = serviceTask.ServiceOrder.TotalPrice,
                PhoneNumber = serviceTask.ServiceOrder.PhoneNumber,
                AppointmentDate = serviceTask.ServiceOrder.AppointmentDate,
                ServiceOrderItems = serviceTask.ServiceOrder.ServiceOrderItems.Select(soi => new ServiceOrderItemResponse
                {
                    ServiceOrderItemId = soi.ServiceOrderItemId,
                    ServicePackageMappingId = soi.ServicePackageMappingId,
                    TotalPrice = soi.TotalPrice,
                    Quantity = soi.Quantity,
                    ServicePackageName = soi.ServicePackageMapping.ServicePackage.PackageName,
                    ProductTypeName = soi.ServicePackageMapping.ProductType.ProductTypeName
                }).ToList(),
                ServiceOrderImageUrl = serviceTask.ServiceOrder.ServiceAssets.Select(se => se.Url).ToList(),
            };
            var lastPayment = serviceTask.ServiceOrder.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            serviceTaskDetailResponse.PaymentMethod = lastPayment.PaymentMethod.ToString();
            serviceTaskDetailResponse.PaymentStatus = lastPayment.Status.ToString();
            return Result<ServiceTaskDetailResponse>.Success(serviceTaskDetailResponse);
        }


        public async Task<Result> UpdateCheckedInStatusAsync(Guid serviceTaskId, Guid staffId)
        {
            var serviceTask = await _repo.GetByIdAsync(serviceTaskId);
            if (serviceTask == null)
            {
                return Result.Failure("Service task not found.", 404);
            }
            if (serviceTask.StaffId != staffId)
            {
                return Result.Failure("You are not assigned to this service task.", 403);
            }
            if (serviceTask.Status != ServiceTaskStatus.Pending)
            {
                return Result.Failure("Service task is not in pending status.", 400);
            }
            serviceTask.Status = ServiceTaskStatus.CheckedIn;
            serviceTask.CheckIn = DateTime.UtcNow;
            var result = await _repo.UpdateAsync(serviceTask);
            return Result.Success($"{result}");
        }
        public async Task<Result> UpdateProcessingStatusAsync(Guid serviceTaskId, Guid staffId)
        {
            var serviceTask = await _repo.GetByIdWithSoAsync(serviceTaskId);
            if (serviceTask == null)
            {
                return Result.Failure("Service task not found.", 404);
            }
            if (serviceTask.StaffId != staffId)
            {
                return Result.Failure("You are not assigned to this service task.", 403);
            }
            if (serviceTask.Status != ServiceTaskStatus.CheckedIn)
            {
                return Result.Failure("Service task is not in checked in status.", 400);
            }
            serviceTask.Status = ServiceTaskStatus.Processing;
            var serviceOrder = serviceTask.ServiceOrder;
            if(serviceOrder.Status != OrderServiceStatus.Confirmed)
            {
                return Result.Failure("Service order must be confirmed before processing service task.", 400);
            }
            serviceTask.ServiceOrder.Status = OrderServiceStatus.Processing;
            _repo.UpdateEntity(serviceTask);
            _soRepo.UpdateEntity(serviceOrder);
            var result = await _unitOfWork.SaveChangeAsync();
            return Result.Success($"{result}");
        }
        public async Task<Result> UpdateForcedCancelledStatusAsync(Guid serviceTaskId, Guid staffId, string staffNote)
        {
            var serviceTask = await _repo.GetByIdWithSoAsync(serviceTaskId);
            if (serviceTask == null)
            {
                return Result.Failure("Service task not found.", 404);
            }
            if (serviceTask.StaffId != staffId)
            {
                return Result.Failure("You are not assigned to this service task.", 403);
            }
            if (serviceTask.Status != ServiceTaskStatus.Processing)
            {
                return Result.Failure("Service task is not in Processing status.", 400);
            }
            var serviceOrder = serviceTask.ServiceOrder;
            if (serviceOrder.Status != OrderServiceStatus.Processing && serviceOrder.Status != OrderServiceStatus.ForcedCancelled)
            {
                return Result.Failure("Service order must be Processing before force cancel service task.", 400);
            }
            serviceTask.Status = ServiceTaskStatus.ForcedCancelled;
            serviceTask.StaffNote = staffNote;
            var result = await _repo.UpdateAsync(serviceTask);
            return Result.Success($"{result}");
        }
        public async Task<Result> UpdateCheckedOutStatusAsync(Guid serviceTaskId, Guid staffId)
        {
            var serviceTask = await _repo.GetByIdAsync(serviceTaskId);
            if (serviceTask == null)
            {
                return Result.Failure("Service task not found.", 404);
            }
            if (serviceTask.StaffId != staffId)
            {
                return Result.Failure("You are not assigned to this service task.", 403);
            }
            if (serviceTask.Status != ServiceTaskStatus.Processing)
            {
                return Result.Failure("Service task is not in processing status.", 400);
            }
            serviceTask.Status = ServiceTaskStatus.CheckedOut;
            serviceTask.CheckOut = DateTime.UtcNow;
            var result = await _repo.UpdateAsync(serviceTask);
            return Result.Success($"{result}");
        }
        public async Task<Result> UpdateCompletedStatusAsync(Guid serviceTaskId, Guid staffId)
        {
            var serviceTask = await _repo.GetByIdWithSoAsync(serviceTaskId);
            if (serviceTask == null)
            {
                return Result.Failure("Service task not found.", 404);
            }
            if (serviceTask.StaffId != staffId)
            {
                return Result.Failure("You are not assigned to this service task.", 403);
            }
            if (serviceTask.Status != ServiceTaskStatus.CheckedOut)
            {
                return Result.Failure("Service task is not in checked out status.", 400);
            }
            if(serviceTask.ServiceEvidences.Count == 0)
            {
                return Result.Failure("Please upload service evidence before completing service task.", 400);
            }
            var serviceOrder = serviceTask.ServiceOrder;
            var payment = serviceOrder.Payments.FirstOrDefault();
            if(payment!.PaymentMethod == PaymentMethod.COD && payment.Status == PaymentStatus.COD)
            {
                payment.Status = PaymentStatus.CODPaid;
            }
            serviceOrder.Status = OrderServiceStatus.Completed;
            serviceTask.Status = ServiceTaskStatus.Completed;
            _repo.UpdateEntity(serviceTask);
            _soRepo.UpdateEntity(serviceOrder);
            _paymentRepo.UpdateEntity(payment);
            var result = await _unitOfWork.SaveChangeAsync();
            return Result.Success($"{result}");
        }

    }
}