using AutoMapper;
using CloudinaryDotNet.Actions;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        public ServiceOrderService(IServicePackageMappingRepository servicePackageMappingRepository, ICustomerRepository customerRepository, IServiceOrderRepository serviceOrderRepository, IVnPayService vnPayService, IMapper mapper, IServiceOrderRepository serviceOrderRepo, IPaymentRepository paymentRepository)
        {
            _servicePackageMappingRepository = servicePackageMappingRepository;
            _customerRepository = customerRepository;
            _serviceOrderRepository = serviceOrderRepository;
            _vnPayService = vnPayService;
            _mapper = mapper;
            _serviceOrderRepo = serviceOrderRepo;
            _paymentRepository = paymentRepository;
        }
        public async Task<Result<OrderServiceResponse>> ReOrderServiceAsync(Guid SoId, Guid customerId, string ipAddress)
        {
            var serviceOrder = await _serviceOrderRepo.GetByIdWithPayment(SoId);
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
            //check if service mapping exist
            var serviceMapping = await _servicePackageMappingRepository.GetByIdAsync(serviceOrderRequest.ServicePackageMappingId);
            if (serviceMapping == null)
            {
                return Result<OrderServiceResponse>.Failure("Service mapping not found", 404);
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
                ServicePackageMappingId = serviceOrderRequest.ServicePackageMappingId,
                CustomNote = serviceOrderRequest.CustomNote,
                Address = serviceOrderRequest.Address,
                City = serviceOrderRequest.City,
                District = serviceOrderRequest.District,
                Ward = serviceOrderRequest.Ward,
                Street = serviceOrderRequest.Street,
                PhoneNumber = serviceOrderRequest.PhoneNumber,
                AppointmentDate = DateTime.UtcNow.AddDays(3), //default appointment date after 3 days, staff contact customer to confirm exact date
                Status = OrderServiceStatus.Pending,
                TotalPrice = serviceMapping.Price + 20000, //add default fee
                CustomerId = customerId
            };
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
            await _serviceOrderRepository.CreateAsync(serviceOrder);

            // Generate VnPay URL after commit (external call, should not be inside transaction)
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

        private string GenerateOrderCode()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = Guid.NewGuid().ToString("N")[..4].ToUpper();
            return $"DGSV-{datePart}-{randomPart}";
        }
        public async Task<Result<PaginatedList<ServiceOrderResponse>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var serviceOrder = await _serviceOrderRepository.GetAllAsync(pageNumber, pageSize);
            if (serviceOrder == null || serviceOrder.TotalCount == 0)
            {
                return Result<PaginatedList<ServiceOrderResponse>>.Failure("No service order found", 404);
            }

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
            var serviceOrder = await _serviceOrderRepository.GetAllAdminAsync(pageNumber, pageSize, searchRequest.OrderCode, searchRequest.PaymentMethod, searchRequest.PaymentStatus);
            if (serviceOrder == null || serviceOrder.TotalCount == 0)
            {
                return Result<PaginatedList<ServiceOrderAdminResponse>>.Failure("No service order found", 404);
            }
            var serviceOrderResponse = _mapper.Map<List<ServiceOrderAdminResponse>>(serviceOrder.Items);
            for (int i = 0; i < serviceOrder.Items.Count; i++)
            {
                var lastPayment = serviceOrder.Items.ElementAt(i).Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                serviceOrderResponse[i].PaymentMethod = lastPayment.PaymentMethod.ToString();
                serviceOrderResponse[i].PaymentStatus = lastPayment.Status.ToString();
            }
            var paginatedResult = new PaginatedList<ServiceOrderAdminResponse>(serviceOrderResponse, serviceOrder.TotalCount, serviceOrder.PageNumber, serviceOrder.PageSize);
            return Result<PaginatedList<ServiceOrderAdminResponse>>.Success(paginatedResult);
        }
        public async Task<Result<ServiceOrderResponse>> GetByIdAsync(Guid serviceOrderId)
        {
            var serviceOrder = await _serviceOrderRepository.GetByIdWithPayment(serviceOrderId);
            if (serviceOrder == null)
            {
                return Result<ServiceOrderResponse>.Failure("service order not found", 404);
            }
            var serviceOrderResponse = _mapper.Map<ServiceOrderResponse>(serviceOrder);
            var lastPayment = serviceOrder.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            serviceOrderResponse.PaymentMethod = lastPayment.PaymentMethod.ToString();
            serviceOrderResponse.PaymentStatus = lastPayment.Status.ToString();
            return Result<ServiceOrderResponse>.Success(serviceOrderResponse);
        }
        public async Task<Result> UpdateServiceOrderAsync(Guid serviceOrderId, ServiceOrderUpdateRequest updateRequest)
        {
            var serviceOrder = await _serviceOrderRepository.GetByIdAsync(serviceOrderId);
            if (serviceOrder == null)
            {
                return Result.Failure("Service order not found", 404);
            }
            _mapper.Map(updateRequest, serviceOrder);
            serviceOrder.UpdatedAt = DateTime.UtcNow;
            var result = await _serviceOrderRepository.UpdateAsync(serviceOrder);
            return Result.Success($"{result}");
        }

        public async Task<Result> RejectPendingServiceOrderAsync(Guid serviceOrderId)
        {
            var serviceOrder = await _serviceOrderRepository.GetByIdWithPayment(serviceOrderId);
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
            var serviceOrder = await _serviceOrderRepository.GetByIdWithPayment(serviceOrderId);
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
           
            var serviceOrder = await _serviceOrderRepository.GetByIdWithPayment(serviceOrderId);
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
                return Result.Failure("Cannot cancel order that has been assigned to staff", 400);
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
    }
}
