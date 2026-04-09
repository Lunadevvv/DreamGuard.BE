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
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ServiceOrderService : IServiceOrderService
    {
        private readonly IServicePackageMappingRepository _servicePackageMappingRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly IVnPayService _vnPayService;
        private readonly IMapper _mapper;
        private readonly IServiceOrderRepository _serviceOrderRepo;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceTaskRepository _serviceTaskRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IUserVoucherRepository _userVoucherRepository;
        private readonly IServiceAssetRepository _serviceAssetRepository;
        public ServiceOrderService(IServicePackageMappingRepository servicePackageMappingRepository, ICustomerRepository customerRepository, IServiceOrderRepository serviceOrderRepository, IVnPayService vnPayService, IMapper mapper, IServiceOrderRepository serviceOrderRepo, IPaymentRepository paymentRepository, IUnitOfWork unitOfWork, IServiceTaskRepository serviceTaskRepository, ICloudinaryService cloudinaryService, IServiceAssetRepository serviceAssetRepository, IUserVoucherRepository userVoucherRepository)
        {
            _servicePackageMappingRepository = servicePackageMappingRepository;
            _customerRepository = customerRepository;
            _serviceOrderRepository = serviceOrderRepository;
            _vnPayService = vnPayService;
            _mapper = mapper;
            _serviceOrderRepo = serviceOrderRepo;
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _serviceTaskRepository = serviceTaskRepository;
            _cloudinaryService = cloudinaryService;
            _serviceAssetRepository = serviceAssetRepository;
            _userVoucherRepository = userVoucherRepository;
        }
        public async Task<Result<OrderServiceResponse>> ReOrderServiceAsync(Guid SoId, Guid customerId, string ipAddress)
        {
            var serviceOrder = await _serviceOrderRepo.GetByIdWithServiceTask(SoId);
            if (serviceOrder == null)
            {
                return Result<OrderServiceResponse>.Failure("Service order not found", 404);
            }
            if (serviceOrder.CustomerId != customerId)
            {
                return Result<OrderServiceResponse>.Failure("You are not the owner of this order", 403);
            }
            if (serviceOrder.Status != OrderServiceStatus.Pending)
            {
                return Result<OrderServiceResponse>.Failure("Only pending order can be reordered", 400);
            }
            var lastPayment = serviceOrder.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            if (lastPayment.Status == PaymentStatus.Paid)
            {
                return Result<OrderServiceResponse>.Failure("This order is already paid", 400);
            }
            if (lastPayment.Status != PaymentStatus.Failed)
            {
                return Result<OrderServiceResponse>.Failure("Only failed payment order can be reordered", 400);
            }
            //check if customer exist
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                return Result<OrderServiceResponse>.Failure("Customer not found", 404);
            }
            var newOrderCode = GenerateOrderCode();
            Payment payment = new Payment
            {
                Amount = serviceOrder.TotalPrice,
                OrderCode = newOrderCode,
                PaymentMethod = lastPayment.PaymentMethod,
                Status = PaymentStatus.Pending,
                Description = $"Payment for service order {serviceOrder.OrderCode}",
                ExpiredAt = DateTime.UtcNow.AddMinutes(5),
                SoId = serviceOrder.SoId
            };
            await _paymentRepository.CreateAsync(payment);

            // Generate VnPay URL after commit (external call, should not be inside transaction)
            string? paymentUrl = null;
            var vnPayRequest = new VnPaymentRequest
            {
                PaymentId = payment.Id.ToString(),
                OrderCode = newOrderCode,
                Description = payment.Description,
                Amount = (int)serviceOrder.TotalPrice,
                IpAddress = ipAddress,
                CreatedDate = payment.CreatedAt
            };
            paymentUrl = _vnPayService.CreatePaymentUrl(vnPayRequest);
            var response = new OrderServiceResponse
            {
                ServiceOrderId = serviceOrder.SoId,
                Price = serviceOrder.TotalPrice,
                PaymentUrl = paymentUrl,
                ExpiredAt = payment.ExpiredAt
            };
            return Result<OrderServiceResponse>.Success(response);
        }
        public async Task<Result<OrderServiceResponse>> OrderServiceAsync(ServiceOrderCreateRequest serviceOrderRequest, Guid customerId, string ipAddress)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var servicePackageMappings = serviceOrderRequest.ServicePackageMappingOrderRequest;
                var servicePackageMappingIds = servicePackageMappings.Select(spm => spm.ServicePackageMappingId).ToList();

                //check duplicate servicePackageMapping id in request
                var servicePackageMappingIdSet = servicePackageMappingIds.ToHashSet();
          
                if (servicePackageMappingIds.Count() != servicePackageMappingIds.Distinct().Count())
                {
                    return Result<OrderServiceResponse>.Failure("Duplicate servicePackageMapping id in request", 400);
                }

                //check if servicePackageMappingIds are valid 
                List<ServicePackageMapping> serviceMappings = await _servicePackageMappingRepository.GetByListIdAsync(servicePackageMappingIds);
                if (serviceMappings == null || !serviceMappings.Any())
                {
                    return Result<OrderServiceResponse>.Failure("No valid service package mapping found for the provided ids", 400);
                }
                var foundIds = serviceMappings.Select(spm => spm.ServicePackageMappingId).ToHashSet();
                var missingIds = servicePackageMappingIds.Where(spm => !foundIds.Contains(spm)).ToList();
                if (missingIds.Any())
                {
                    return Result<OrderServiceResponse>.Failure($"The following service package mapping ids are invalid: {string.Join(", ", missingIds)}", 400);
                }



                //check if customer exist
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                {
                    return Result<OrderServiceResponse>.Failure("Customer not found", 404);
                }


                var serviceOrder = new ServiceOrder
                {
                    OrderCode = GenerateOrderCode(),
                    ReceiverName = serviceOrderRequest.ReceiverName,
                    CustomerNote = serviceOrderRequest.CustomerNote,
                    CustomerId = customerId,
                    Address = serviceOrderRequest.Address,
                    PhoneNumber = serviceOrderRequest.PhoneNumber,
                    AppointmentDate = serviceOrderRequest.AppointmentDate!.Value,
                    Status = OrderServiceStatus.Pending,
                };


                //Add service order items
                var mappingDict = serviceMappings.ToDictionary(m => m.ServicePackageMappingId);
                foreach (var spm in servicePackageMappings)
                {
                    mappingDict.TryGetValue(spm.ServicePackageMappingId, out var mapping);
                    var serviceOrderItem = new ServiceOrderItem
                    {
                        SoId = serviceOrder.SoId,
                        ServicePackageMappingId = mapping!.ServicePackageMappingId,
                        Quantity = spm.Quantity-1,
                        TotalPrice = mapping.Price + (mapping.ProductType.AddPrice * (spm.Quantity-1)),
                    };
                    serviceOrder.ServiceOrderItems.Add(serviceOrderItem);
                    serviceOrder.SubTotalPrice += serviceOrderItem.TotalPrice;
                }

                // For now, no discount or extra fee, so total price = sub total price
                serviceOrder.TotalPrice = serviceOrder.SubTotalPrice; 
                //Add voucher discount if have voucher in request
                if (serviceOrderRequest.UserVoucherId != null)
                {
                    var userVoucher = await _userVoucherRepository.GetByIdAsync(serviceOrderRequest.UserVoucherId.Value);
                    if (userVoucher == null)
                    {
                        return Result<OrderServiceResponse>.Failure("User voucher not found", 404);
                    }
                    if (userVoucher.CustomerId != customerId)
                    {
                        return Result<OrderServiceResponse>.Failure("This voucher does not belong to the customer", 400);
                    }

                    if (userVoucher.ExpiredAt < DateTime.UtcNow)
                    {
                        return Result<OrderServiceResponse>.Failure("This voucher has expired", 400);
                    }
                    var voucher = userVoucher.Voucher;
                    if (voucher == null || !voucher.IsActive || voucher.EndDate < DateTime.UtcNow)
                    {
                        return Result<OrderServiceResponse>.Failure("Voucher is no longer active.", 400);
                    }
                    // execute db-level atomic operation to avoid race-condition with another order using the same voucher
                    // also check if voucher is used or not by checking the affected rows
                    var updateResult = await _userVoucherRepository.MarkAsUsedAsync(userVoucher.UserVoucherId);
                    if (!updateResult) // false means no row updated 
                    {
                        return Result<OrderServiceResponse>.Failure("Voucher already used", 400);
                    }
                    var subTotalPrice = serviceOrder.SubTotalPrice;
                    decimal discountAmount = 0;
                    discountAmount = subTotalPrice * voucher.DiscountValue;
                    discountAmount = Math.Min(discountAmount, voucher.MaxDiscountAmount);
                    discountAmount = Math.Min(discountAmount, subTotalPrice); // Discount cannot exceed subtotal

                    if (voucher.VoucherType == DAL.Constants.VoucherType.Product)
                    {
                        return Result<OrderServiceResponse>.Failure("This voucher is specifically for products only.", 400);
                    }

                    serviceOrder.UserVoucherId = userVoucher.UserVoucherId;
                    serviceOrder.TotalPrice = Math.Max(serviceOrder.SubTotalPrice - discountAmount, 0); // Total price cannot be negative
                }

                Payment payment = new Payment
                {
                    Amount = serviceOrder.TotalPrice,
                    OrderCode = serviceOrder.OrderCode,
                    PaymentMethod = serviceOrderRequest.PaymentMethod,
                    Status = serviceOrderRequest.PaymentMethod == PaymentMethod.VnPay ? PaymentStatus.Pending : PaymentStatus.COD,
                    Description = $"Payment for service order {serviceOrder.OrderCode}",
                    ExpiredAt = DateTime.UtcNow.AddMinutes(5)
                };
                serviceOrder.Payments.Add(payment);
                _serviceOrderRepository.AddEntity(serviceOrder);

                await _unitOfWork.SaveChangeAsync();
                await transaction.CommitAsync();

                // Generate VnPay URL after commit
                // (external call, should not be inside transaction)
                string? paymentUrl = null;
                if (serviceOrderRequest.PaymentMethod == PaymentMethod.VnPay)
                {
                    var vnPayRequest = new VnPaymentRequest
                    {
                        PaymentId = payment.Id.ToString(),
                        OrderCode = serviceOrder.OrderCode,
                        Description = payment.Description,
                        Amount = (int)serviceOrder.TotalPrice,
                        IpAddress = ipAddress,
                        CreatedDate = payment.CreatedAt
                    };
                    paymentUrl = _vnPayService.CreatePaymentUrl(vnPayRequest);
                }

                var response = new OrderServiceResponse
                {
                    ServiceOrderId = serviceOrder.SoId,
                    Price = serviceOrder.TotalPrice,
                    PaymentUrl = paymentUrl,
                    ExpiredAt = payment.ExpiredAt
                };

                return Result<OrderServiceResponse>.Success(response);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private string GenerateOrderCode()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = Guid.NewGuid().ToString("N")[..4].ToUpper();
            return $"DGSV-{datePart}-{randomPart}";
        }
        public async Task<Result<PaginatedList<ServiceOrderResponse>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var serviceOrder = await _serviceOrderRepository.GetAllAsync(pageNumber, pageSize);
            var serviceOrderResponse = _mapper.Map<List<ServiceOrderResponse>>(serviceOrder.Items);
            for (int i = 0; i < serviceOrder.Items.Count; i++)
            {
                var lastPayment = serviceOrder.Items.ElementAt(i).Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                serviceOrderResponse[i].PaymentMethod = lastPayment.PaymentMethod.ToString();
                serviceOrderResponse[i].PaymentStatus = lastPayment.Status.ToString();
            }
            var paginatedResult = new PaginatedList<ServiceOrderResponse>(serviceOrderResponse, serviceOrder.TotalCount, serviceOrder.PageNumber, serviceOrder.PageSize);
            return Result<PaginatedList<ServiceOrderResponse>>.Success(paginatedResult);
        }

        public async Task<Result<PaginatedList<ServiceOrderAdminResponse>>> GetAllByAdminAsync(int pageNumber, int pageSize, ServiceOrderSearchRequest searchRequest)
        {
            var serviceOrder = await _serviceOrderRepository.GetAllAdminAsync(pageNumber, pageSize, searchRequest.ServiceOrderId , searchRequest.OrderCode, searchRequest.PaymentMethod, searchRequest.PaymentStatus);

            var ServiceOrderAdminResponse = _mapper.Map<List<ServiceOrderAdminResponse>>(serviceOrder.Items);
            for (int i = 0; i < serviceOrder.Items.Count; i++)
            {
                var lastPayment = serviceOrder.Items.ElementAt(i).Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                ServiceOrderAdminResponse[i].PaymentMethod = lastPayment.PaymentMethod.ToString();
                ServiceOrderAdminResponse[i].PaymentStatus = lastPayment.Status.ToString();
            }
            var paginatedResult = new PaginatedList<ServiceOrderAdminResponse>(ServiceOrderAdminResponse, serviceOrder.TotalCount, serviceOrder.PageNumber, serviceOrder.PageSize);
            return Result<PaginatedList<ServiceOrderAdminResponse>>.Success(paginatedResult);
        }
        public async Task<Result<ServiceOrderDetailResponse>> GetByIdAsync(Guid serviceOrderId, Guid customerId, string role)
        {
            var serviceOrder = await _serviceOrderRepository.GetByIdWithDetail(serviceOrderId);
            if (serviceOrder == null)
            {
                return Result<ServiceOrderDetailResponse>.Failure("service order not found", 404);
            }
            if(role != Role.Admin && role != Role.Manager && serviceOrder.CustomerId != customerId)
            {
                return Result<ServiceOrderDetailResponse>.Failure("No permission for access this order", 403);
            }

            var serviceOrderResponse = new ServiceOrderDetailResponse
            {
                SoId = serviceOrder.SoId,
                CustomerId = customerId,
                OrderCode = serviceOrder.OrderCode,
                ReceiverName = serviceOrder.ReceiverName,
                Address = serviceOrder.Address,
                PhoneNumber = serviceOrder.PhoneNumber,
                CustomerNote = serviceOrder.CustomerNote,
                AppointmentDate = serviceOrder.AppointmentDate,
                Status = serviceOrder.Status,
                TotalPrice = serviceOrder.TotalPrice,
                SubTotalPrice = serviceOrder.SubTotalPrice,
                CreatedAt = serviceOrder.CreatedAt,
                UpdatedAt = serviceOrder.UpdatedAt,
                ServiceOrderItems = serviceOrder.ServiceOrderItems.Select(soi => new ServiceOrderItemResponse
                {
                    ServiceOrderItemId = soi.ServiceOrderItemId,
                    ServicePackageMappingId = soi.ServicePackageMappingId,
                    TotalPrice = soi.TotalPrice,
                    Quantity = soi.Quantity,
                    ServicePackageName = soi.ServicePackageMapping.ServicePackage.PackageName,
                    ProductTypeName = soi.ServicePackageMapping.ProductType.ProductTypeName
                }).ToList(),
                ImageUrl = serviceOrder.ServiceAssets.Select(sa => sa.Url).ToList(),
                Rating = serviceOrder.Rating == null ? null : _mapper.Map<RatingResponse>(serviceOrder.Rating)
            };
            var staff = serviceOrder.ServiceTask?.Staff;
            if(staff != null)
            {
                serviceOrderResponse.Staff = _mapper.Map<StaffResponse>(staff);
            }

            var lastPayment = serviceOrder.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            serviceOrderResponse.PaymentMethod = lastPayment.PaymentMethod.ToString();
            serviceOrderResponse.PaymentStatus = lastPayment.Status.ToString();
            return Result<ServiceOrderDetailResponse>.Success(serviceOrderResponse);
        }
        public async Task<Result> UpdateServiceOrderAsync(Guid customerId, Guid serviceOrderId, ServiceOrderUpdateRequest updateRequest)
        {
            var serviceOrder = await _serviceOrderRepository.GetByIdAsync(serviceOrderId);
            if (serviceOrder == null)
            {
                return Result.Failure("Service order not found", 404);
            }
            if (serviceOrder.CustomerId != customerId)
            {
                return Result.Failure("You are not the owner of this order", 403);
            }
            if (serviceOrder.Status == OrderServiceStatus.Processing || serviceOrder.Status == OrderServiceStatus.Completed)
            {
                return Result.Failure("Can't update Processing or Completed Order", 400);
            }
            _mapper.Map(updateRequest, serviceOrder);
            serviceOrder.UpdatedAt = DateTime.UtcNow;
            var result = await _serviceOrderRepository.UpdateAsync(serviceOrder);
            return Result.Success($"{result}");
        }

        public async Task<Result> RejectPendingServiceOrderAsync(Guid serviceOrderId)
        {
            var serviceOrder = await _serviceOrderRepository.GetByIdWithServiceTask(serviceOrderId);
            if (serviceOrder == null)
            {
                return Result.Failure("Service order not found", 404);
            }
            if (serviceOrder.Status != OrderServiceStatus.Pending)
            {
                return Result.Failure("Only pending order can be rejected", 400);
            }
            var lastPayment = serviceOrder.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            if (lastPayment != null && lastPayment.Status == PaymentStatus.Paid)
            {
                serviceOrder.Status = OrderServiceStatus.Refund;
            }
            else
            {
                serviceOrder.Status = OrderServiceStatus.Rejected;
            }
            serviceOrder.UpdatedAt = DateTime.UtcNow;
            var result = await _serviceOrderRepository.UpdateAsync(serviceOrder);
            return Result.Success($"{result}");
        }

        public async Task<Result> ConfirmPendingServiceOrderAsync(Guid serviceOrderId)
        {
            var serviceOrder = await _serviceOrderRepository.GetByIdWithServiceTask(serviceOrderId);
            if (serviceOrder == null)
            {
                return Result.Failure("Service order not found", 404);
            }
            if (serviceOrder.Status != OrderServiceStatus.Pending)
            {
                return Result.Failure("Only pending order can be confirmed", 400);
            }
            var lastPayment = serviceOrder.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            if (lastPayment!.Status != PaymentStatus.Paid && lastPayment.PaymentMethod != PaymentMethod.COD)
            {
                return Result.Failure("Only paid order can be confirmed", 400);
            }
            serviceOrder.Status = OrderServiceStatus.Confirmed;
            serviceOrder.UpdatedAt = DateTime.UtcNow;
            var result = await _serviceOrderRepository.UpdateAsync(serviceOrder);
            return Result.Success($"{result}");
        }

        public async Task<Result> CancelPendingServiceOrderAsync(Guid customerId, Guid serviceOrderId)
        {
           
            var serviceOrder = await _serviceOrderRepository.GetByIdWithServiceTask(serviceOrderId);
            if (serviceOrder == null)
            {
                return Result.Failure("Service order not found", 404);
            }
            if (serviceOrder.CustomerId != customerId)
            {
                return Result.Failure("You are not the owner of this order", 403);
            }
            if (serviceOrder.Status != OrderServiceStatus.Pending)
            {
                return Result.Failure("Only pending order can be cancelled", 400);
            }
            var lastPayment = serviceOrder.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            if (lastPayment != null && lastPayment.Status == PaymentStatus.Paid)
            {
                serviceOrder.Status = OrderServiceStatus.Refund;
            }
            else
            {
                serviceOrder.Status = OrderServiceStatus.Cancelled;
            }
            serviceOrder.UpdatedAt = DateTime.UtcNow;
            var result = await _serviceOrderRepository.UpdateAsync(serviceOrder);
            return Result.Success($"{result}");
        }

        public async Task<Result> ManagerCancelConfirmedServiceOrderAsync(Guid serviceOrderId)
        {
            var serviceOrder = await _serviceOrderRepository.GetByIdWithServiceTask(serviceOrderId);
            if (serviceOrder == null)
            {
                return Result.Failure("Service order not found", 404);
            }
            if (serviceOrder.Status != OrderServiceStatus.Confirmed)
            {
                return Result.Failure("Only confirmed order can be cancelled by manager", 400);
            }
            if (serviceOrder.ServiceTask != null)
            {
                var serviceTask = serviceOrder.ServiceTask;
                serviceTask.Status = ServiceTaskStatus.Cancelled;
                _serviceTaskRepository.UpdateEntity(serviceTask);
            }
            var lastPayment = serviceOrder.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            if (lastPayment != null && lastPayment.Status == PaymentStatus.Paid)
            {
                serviceOrder.Status = OrderServiceStatus.Refund;
            }
            else
            {
                serviceOrder.Status = OrderServiceStatus.Cancelled;
            }
            serviceOrder.UpdatedAt = DateTime.UtcNow;
            _serviceOrderRepo.UpdateEntity(serviceOrder);
            var result = await _unitOfWork.SaveChangeAsync();           
            return Result.Success($"{result}");
        }
        public async Task<Result> ManagerCancelProcessingServiceOrderAsync(Guid serviceOrderId)
        {
            var serviceOrder = await _serviceOrderRepository.GetByIdWithServiceTask(serviceOrderId);
            if (serviceOrder == null)
            {
                return Result.Failure("Service order not found", 404);
            }
            if (serviceOrder.Status != OrderServiceStatus.Processing)
            {
                return Result.Failure("Only processing order can be cancelled by manager", 400);
            }
            serviceOrder.Status = OrderServiceStatus.ForcedCancelled;
            serviceOrder.UpdatedAt = DateTime.UtcNow;
            var result = await _serviceOrderRepository.UpdateAsync(serviceOrder);
            return Result.Success($"{result}");
        }

        public async Task<Result> UploadServiceAsset(Guid serviceOrderId, ServiceAssetCreateRequest assetCreateRequest)
        {
            var serviceOrder = await _serviceOrderRepository.GetByIdAsync(serviceOrderId);
            if (serviceOrder == null)
            {
                return Result.Failure("Service order not found", 404);
            }
            if (assetCreateRequest.Files == null || !assetCreateRequest.Files.Any())
            {
                return Result.Failure("No files to upload", 400);
            }
            var files = assetCreateRequest.Files;

            foreach (var file in files)
            {
                var uploadImageResult = await _cloudinaryService.UploadImageAsync(file, "SERVICE_ASSET_FOLDER");
                if (!uploadImageResult.Succeeded)
                {
                    return Result.Failure($"Failed to upload image: {uploadImageResult.Error}", 500);
                }
                var asset = new ServiceAsset
                {
                    ServiceOrderId = serviceOrderId,
                    Url = uploadImageResult.Data!.Url,
                    Type = file.ContentType,
                    PublicId = uploadImageResult.Data.PublicId
                };
                _serviceAssetRepository.AddEntity(asset);
            }
            var result = await _unitOfWork.SaveChangeAsync();
            return Result.Success($"{result}");
        }
    }
}
