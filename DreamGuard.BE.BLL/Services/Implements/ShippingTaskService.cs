using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ShippingTaskService : IShippingTaskService
    {
        private readonly IShippingTaskRepository _taskRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IShippingEvidenceRepository _evidenceRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVnPayService _vnPayService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ISystemConfigRepository _systemConfigRepository;

        public ShippingTaskService(
            IShippingTaskRepository taskRepository,
            IOrderRepository orderRepository,
            IStaffRepository staffRepository,
            IShippingEvidenceRepository evidenceRepository,
            IInventoryService inventoryService,
            IUnitOfWork unitOfWork,
            IVnPayService vnPayService,
            IPaymentRepository paymentRepository,
            ICustomerRepository customerRepository,
            ISystemConfigRepository systemConfigRepository)
        {
            _taskRepository = taskRepository;
            _orderRepository = orderRepository;
            _staffRepository = staffRepository;
            _evidenceRepository = evidenceRepository;
            _inventoryService = inventoryService;
            _unitOfWork = unitOfWork;
            _vnPayService = vnPayService;
            _paymentRepository = paymentRepository;
            _customerRepository = customerRepository;
            _systemConfigRepository = systemConfigRepository;
        }

        public async Task<Result<ShippingTaskResponse>> CreateShippingTaskAsync(ShippingTaskCreateRequest request)
        {
            var staff = await _staffRepository.GetByIdAsync(request.StaffId);
            if (staff == null || staff.Position != Role.DeliveryStaff)
            {
                return Result<ShippingTaskResponse>.Failure("Invalid staff or staff is not a Delivery Staff.", 400);
            }

            var order = await _orderRepository.GetOrderWithItemsForUpdateAsync(request.OrderId);
            if (order == null)
            {
                return Result<ShippingTaskResponse>.Failure("Order not found.", 404);
            }

            if (order.Status != OrderStatus.Confirmed && order.Status != OrderStatus.Processing)
            {
                return Result<ShippingTaskResponse>.Failure(
                    $"Cannot create shipping task. Order must be in 'Confirmed' or 'Processing' status but is currently '{order.Status}'.", 400);
            }

            var existingTask = await _taskRepository.GetTaskByOrderIdAsync(order.Id);
            if (existingTask != null)
            {
                return Result<ShippingTaskResponse>.Failure("Order already has a shipping task.", 400);
            }

            var task = new ShippingTask
            {
                ShippingTaskId = Guid.NewGuid(),
                StaffId = request.StaffId,
                OrderId = request.OrderId,
                Status = ShippingTaskStatus.Pending,
                StaffNote = string.Empty
            };

            await _taskRepository.CreateAsync(task);
            
            // Re-fetch to map properly with navigation properties
            task.Staff = staff;
            task.Order = order;

            return Result<ShippingTaskResponse>.Success(MapToResponse(task));
        }

        public async Task<Result> ReassignStaffAsync(Guid taskId, ReassignStaffRequest request)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                return Result.Failure("Shipping task not found.", 404);
            }

            if (task.Status == ShippingTaskStatus.Delivered || task.Status == ShippingTaskStatus.Returned || task.Status == ShippingTaskStatus.Cancelled)
            {
                return Result.Failure($"Cannot reassign staff for task in status '{task.Status}'.", 400);
            }

            var newStaff = await _staffRepository.GetByIdAsync(request.NewStaffId);
            if (newStaff == null || newStaff.Position != Role.DeliveryStaff)
            {
                return Result.Failure("Invalid staff or staff is not a Delivery Staff.", 400);
            }

            task.StaffId = request.NewStaffId;
            await _taskRepository.UpdateAsync(task);

            return Result.Success("Staff reassigned successfully.");
        }

        public async Task<Result> UpdateTaskToDeliveringAsync(Guid taskId, Guid staffId, StartShippingRequest request)
        {
            if (request.EvidenceUrls == null || !request.EvidenceUrls.Any())
            {
                return Result.Failure("At least one evidence image is required to start delivery.", 400);
            }

            var task = await _taskRepository.GetTaskWithDetailsForUpdateAsync(taskId);
            if(task.OrderId == null)
            {
                return Result.Failure("Associated order not found for this task.", 404);
            }
            if (task == null) return Result.Failure("Shipping task not found.", 404);

            if (task.StaffId != staffId) return Result.Failure("You are not assigned to this task.", 403);

            if (task.Status != ShippingTaskStatus.Pending) return Result.Failure($"Cannot start delivering from status '{task.Status}'.", 400);

            var order = await _orderRepository.GetOrderWithItemsForUpdateAsync(task.OrderId!.Value);
            if (order == null) return Result.Failure("Order not found.", 404);

            if (order.Status != OrderStatus.Processing && order.Status != OrderStatus.ExchangeRequested)
                return Result.Failure($"Cannot start delivering. Order must be in 'Processing' or 'ExchangeRequested' status, but is currently '{order?.Status}'.", 400);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Update Order Status
                if(order.Status == OrderStatus.Processing)
                {
                    order.Status = OrderStatus.Shipping;
                }
                else if(order.Status == OrderStatus.ExchangeRequested)
                {
                    order.Status = OrderStatus.Shipping_Replacement;
                }
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);

                // Update Task Status
                task.Status = ShippingTaskStatus.Delivering;
                task.ShippingDate = DateTime.UtcNow;
                await _taskRepository.UpdateAsync(task);

                // Save Evidence
                foreach (var url in request.EvidenceUrls)
                {
                    await _evidenceRepository.CreateAsync(new ShippingEvidence
                    {
                        EvidenceId = Guid.NewGuid(),
                        ShippingTaskId = task.ShippingTaskId,
                        EvidenceUrl = url,
                        EvidenceType = "PickedUp",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await transaction.CommitAsync();
                return Result.Success("Task is now in Delivering status.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result.Failure("Failed to update status.", 500);
            }
        }

        public async Task<Result> UpdateTaskToArrivedAsync(Guid taskId, Guid staffId)
        {
            var task = await _taskRepository.GetTaskWithDetailsForUpdateAsync(taskId);
            if (task == null) return Result.Failure("Shipping task not found.", 404);

            if (task.StaffId != staffId) return Result.Failure("You are not assigned to this task.", 403);

            if (task.Status != ShippingTaskStatus.Delivering)
                return Result.Failure($"Cannot arrive from status '{task.Status}'.", 400);

            task.Status = ShippingTaskStatus.Arrived;
            await _taskRepository.UpdateAsync(task);

            return Result.Success("Task is now Arrived.");
        }

        public async Task<Result> CompleteShippingAsync(Guid taskId, Guid staffId, CompleteShippingRequest request)
        {
            if (request.EvidenceUrls == null || !request.EvidenceUrls.Any())
            {
                return Result.Failure("At least one evidence image is required.", 400);
            }

            var task = await _taskRepository.GetTaskWithDetailsForUpdateAsync(taskId);
            if (task == null) return Result.Failure("Shipping task not found.", 404);

            if (task.StaffId != staffId) return Result.Failure("You are not assigned to this task.", 403);

            if (task.Status != ShippingTaskStatus.Arrived) return Result.Failure($"Cannot complete from status '{task.Status}'.", 400);

            var order = await _orderRepository.GetOrderWithItemsForUpdateAsync(task.OrderId!.Value);
            if (order == null || (order.Status != OrderStatus.Shipping && order.Status != OrderStatus.Shipping_Replacement)) return Result.Failure("Order is not in Shipping status.", 400);

            var payment = await _paymentRepository.GetPaymentByOrderIdAsync(order.Id);
            bool isVnPay = payment != null && payment.PaymentMethod == PaymentMethod.VnPay;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Update Order Status
                order.Status = isVnPay ? OrderStatus.Completed : OrderStatus.Delivered;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);

                // Award points if order is Completed
                if (order.Status == OrderStatus.Completed)
                {
                    var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
                    if (customer != null)
                    {
                        var config = await _systemConfigRepository.GetByKeyAsync("OrderCoinPercent");
                        decimal percent = 1.0m; // default 1%
                        if (config != null && decimal.TryParse(config.ConfigValue, out decimal parsed))
                        {
                            percent = parsed;
                        }
                        int coinsEarned = (int)(order.TotalAmount * percent / 100);
                        customer.MemberCoin += coinsEarned;
                        _customerRepository.UpdateEntity(customer);
                    }
                }

                // Update Task Status
                task.Status = ShippingTaskStatus.Delivered;
                task.CompletionDate = DateTime.UtcNow;
                await _taskRepository.UpdateAsync(task);

                // Save Evidences
                foreach (var url in request.EvidenceUrls)
                {
                    await _evidenceRepository.CreateAsync(new ShippingEvidence
                    {
                        EvidenceId = Guid.NewGuid(),
                        ShippingTaskId = task.ShippingTaskId,
                        EvidenceUrl = url,
                        EvidenceType = "Delivered",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await transaction.CommitAsync();
                return Result.Success("Shipping completed successfully.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result.Failure("Failed to complete shipping.", 500);
            }
        }

        public async Task<Result> FailShippingAsync(Guid taskId, Guid staffId, FailShippingRequest request)
        {
            if (request.EvidenceUrls == null || !request.EvidenceUrls.Any())
            {
                return Result.Failure("At least one evidence image is required.", 400);
            }

            var task = await _taskRepository.GetTaskWithDetailsForUpdateAsync(taskId);
            if(task.OrderId == null)
            {
                return Result.Failure("Associated order not found for this task.", 404);
            }
            if (task == null) return Result.Failure("Shipping task not found.", 404);

            if (task.StaffId != staffId) return Result.Failure("You are not assigned to this task.", 403);

            if (task.Status != ShippingTaskStatus.Arrived && task.Status != ShippingTaskStatus.Delivering)
            {
                return Result.Failure($"Cannot return from status '{task.Status}'.", 400);
            }

            var order = await _orderRepository.GetOrderWithItemsForUpdateAsync(task.OrderId!.Value);
            if (order == null) return Result.Failure("Order not found.", 404);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Unhappy Case: Order status -> Returning (No refund or restock yet)
                order.Status = OrderStatus.Returning;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);

                // Task status -> Returning
                task.Status = ShippingTaskStatus.Returning;
                task.StaffNote = request.Reason;
                // Don't set CompletionDate yet, the manager finishes it.
                await _taskRepository.UpdateAsync(task);

                // Save Evidences
                foreach (var url in request.EvidenceUrls)
                {
                    await _evidenceRepository.CreateAsync(new ShippingEvidence
                    {
                        EvidenceId = Guid.NewGuid(),
                        ShippingTaskId = task.ShippingTaskId,
                        EvidenceUrl = url,
                        EvidenceType = "Returning",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await transaction.CommitAsync();
                return Result.Success("Shipping marked as returning successfully. Pending manager review.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result.Failure("Failed to mark shipping as returning.", 500);
            }
        }

        public async Task<Result> ProcessReturnedOrderAsync(Guid taskId, ProcessReturnedRequest request)
        {
            var task = await _taskRepository.GetTaskWithDetailsForUpdateAsync(taskId);
            if (task == null) return Result.Failure("Shipping task not found.", 404);
            if (task.OrderId == null) return Result.Failure("Associated order not found for this task.", 404);
            if (task.Status != ShippingTaskStatus.Returning)
            {
                return Result.Failure($"Task must be in 'Returning' status to process. Current status: '{task.Status}'.", 400);
            }

            var order = await _orderRepository.GetOrderWithItemsForUpdateAsync(task.OrderId!.Value);
            if (order == null) return Result.Failure("Order not found.", 404);

            bool isDamaged = request.DamagedItems != null && request.DamagedItems.Any(d => d.DamagedQuantity > 0);

            //validate damaged items quantity
            if (isDamaged)
            {
                foreach (var damageReq in request.DamagedItems)
                {
                    var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == damageReq.OrderItemId);
                    if (orderItem == null)
                    {
                        return Result.Failure($"Order item with ID {damageReq.OrderItemId} not found in the order.", 404);
                    }
                    if (damageReq.DamagedQuantity < 0 || damageReq.DamagedQuantity > orderItem.Quantity)
                    {
                        return Result.Failure($"Invalid damaged quantity for order item {orderItem.Id}. It must be between 0 and {orderItem.Quantity}.", 400);
                    }
                }
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Update Order Status
                order.Status = isDamaged ? OrderStatus.RefundedAndDamaged : OrderStatus.RefundedAndRestocked;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);

                // Task status
                task.Status = isDamaged ? ShippingTaskStatus.RefundedAndDamaged : ShippingTaskStatus.RefundedAndRestocked;
                task.CompletionDate = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(request.DamageNote))
                {
                    task.StaffNote = string.IsNullOrEmpty(task.StaffNote) 
                        ? $"Manager Note: {request.DamageNote}" 
                        : $"{task.StaffNote} | Manager Note: {request.DamageNote}";
                }
                await _taskRepository.UpdateAsync(task);

                // --- REFUND VNPay ---
                var payment = await _paymentRepository.GetPaymentByOrderIdAsync(order.Id);
                if (payment != null && payment.PaymentMethod == PaymentMethod.VnPay && payment.Status == PaymentStatus.Paid && payment.PaymentType == PaymentType.Purchase)
                {
                    var refundReq = new VnPaymentRefundRequest
                    {
                        OrderId = payment.Id.ToString(), // Or TxnRef
                        Amount = payment.Amount,
                        PaymentDate = payment.CreatedAt,
                        CreateBy = "Manager",
                        IpAddress = "127.0.0.1",
                        TransactionNo = "0"
                    };

                    // var refundRes = await _vnPayService.RefundPaymentAsync(refundReq);
                    // if (!refundRes.Success)
                    // {
                    //     await transaction.RollbackAsync();
                    //     return Result.Failure($"VnPay Refund failed: {refundRes.Message} (Code: {refundRes.ResponseCode})", 400);
                    // }

                    var refundPayment = new Payment
                    {
                        Id = Guid.NewGuid(),
                        OrderCode = order.OrderCode,
                        POrderId = order.Id,
                        Status = PaymentStatus.Paid,
                        PaymentType = PaymentType.Refund,
                        Amount = payment.Amount,
                        Description = $"Refund for Order {order.OrderCode}.",
                        PaymentMethod = PaymentMethod.VnPay,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ExpiredAt = DateTime.UtcNow.AddMinutes(5)
                    };
                    await _paymentRepository.CreateAsync(refundPayment);
                }

                // Rollback Stock (Split between Normal and Defect)
                foreach (var item in order.OrderItems)
                {
                    var damageReq = request.DamagedItems?.FirstOrDefault(d => d.OrderItemId == item.Id);
                    int damagedQty = damageReq != null ? damageReq.DamagedQuantity : 0;
                    
                    if (damagedQty > item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return Result.Failure($"Damaged quantity ({damagedQty}) cannot exceed ordered quantity ({item.Quantity}) for item {item.Id}.", 400);
                    }

                    int healthyQty = item.Quantity - damagedQty;

                    if (item.ProductVariantId.HasValue)
                    {
                        if (healthyQty > 0)
                        {
                            var hr = await _inventoryService.RestoreVariantStockAsync(item.ProductVariantId.Value, healthyQty);
                            if (!hr.Succeeded) { await transaction.RollbackAsync(); return Result.Failure(hr.Error!, hr.StatusCode); }
                        }
                        if (damagedQty > 0)
                        {
                            var dr = await _inventoryService.RestoreDefectVariantStockAsync(item.ProductVariantId.Value, damagedQty);
                            if (!dr.Succeeded) { await transaction.RollbackAsync(); return Result.Failure(dr.Error!, dr.StatusCode); }
                        }
                    }
                    else if (item.ComboId.HasValue)
                    {
                        if (healthyQty > 0)
                        {
                            var hr = await _inventoryService.RestoreComboStockAsync(item.ComboId.Value, healthyQty);
                            if (!hr.Succeeded) { await transaction.RollbackAsync(); return Result.Failure(hr.Error!, hr.StatusCode); }
                        }
                        if (damagedQty > 0)
                        {
                            var dr = await _inventoryService.RestoreDefectComboStockAsync(item.ComboId.Value, damagedQty);
                            if (!dr.Succeeded) { await transaction.RollbackAsync(); return Result.Failure(dr.Error!, dr.StatusCode); }
                        }
                    }
                }

                // Save Evidences (if damaged)
                if (isDamaged && request.EvidenceUrls != null && request.EvidenceUrls.Any())
                {
                    foreach (var url in request.EvidenceUrls)
                    {
                        var evidence = new ShippingEvidence
                        {
                            EvidenceId = Guid.NewGuid(),
                            ShippingTaskId = task.ShippingTaskId,
                            EvidenceUrl = url,
                            EvidenceType = "DamageReport",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _evidenceRepository.CreateAsync(evidence);
                    }
                }

                await transaction.CommitAsync();
                return Result.Success("Return processed successfully.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result.Failure("Failed to process returned order.", 500);
            }
        }

        public async Task<Result> ProcessExchangeOrderAsync(Guid taskId, ProcessExchangeRequest request)
        {
            var task = await _taskRepository.GetTaskWithDetailsForUpdateAsync(taskId);
            if (task == null) return Result.Failure("Shipping task not found.", 404);
            if (task.OrderId == null) return Result.Failure("Associated order not found for this task.", 404);
            if (task.Status != ShippingTaskStatus.Returning)
            {
                return Result.Failure($"Task must be in 'Returning' status to process. Current status: '{task.Status}'.", 400);
            }

            var order = await _orderRepository.GetOrderWithItemsForUpdateAsync(task.OrderId!.Value);
            if (order == null) return Result.Failure("Order not found.", 404);

            bool isDamaged = request.DamagedItems != null && request.DamagedItems.Any(d => d.DamagedQuantity > 0);

            //validate damaged items quantity
            if (isDamaged)
            {
                foreach (var damageReq in request.DamagedItems!)
                {
                    var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == damageReq.OrderItemId);
                    if (orderItem == null)
                    {
                        return Result.Failure($"Order item with ID {damageReq.OrderItemId} not found in the order.", 404);
                    }
                    if (damageReq.DamagedQuantity <= 0 || damageReq.DamagedQuantity > orderItem.Quantity)
                    {
                        return Result.Failure($"Invalid damaged quantity for order item {orderItem.Id}. It must be between 1 and {orderItem.Quantity}.", 400);
                    }
                }
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Update Order Status
                order.Status = OrderStatus.ExchangeRequested;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);

                // Task status (Closed)
                task.Status = ShippingTaskStatus.ExchangeRequested;
                task.CompletionDate = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(request.ExchangeNote))
                {
                    task.StaffNote = string.IsNullOrEmpty(task.StaffNote) 
                        ? $"Manager Note (Exchange): {request.ExchangeNote}" 
                        : $"{task.StaffNote} | Manager Note (Exchange): {request.ExchangeNote}";
                }
                await _taskRepository.UpdateAsync(task);

                // Add Defect Stock & Deduct New Stock for Replacement
                foreach (var item in order.OrderItems)
                {
                    var damageReq = request.DamagedItems?.FirstOrDefault(d => d.OrderItemId == item.Id);
                    int damagedQty = damageReq != null ? damageReq.DamagedQuantity : 0;
                    
                    if (damagedQty > 0)
                    {
                        if (item.ProductVariantId.HasValue)
                        {
                            // Put broken into defect
                            var dr = await _inventoryService.RestoreDefectVariantStockAsync(item.ProductVariantId.Value, damagedQty);
                            if (!dr.Succeeded) { await transaction.RollbackAsync(); return Result.Failure(dr.Error!, dr.StatusCode); }
                            
                            // Take new one from normal stock
                            var ds = await _inventoryService.DeductVariantStockAsync(item.ProductVariantId.Value, damagedQty);
                            if (!ds.Succeeded) { await transaction.RollbackAsync(); return Result.Failure(ds.Error!, ds.StatusCode); }
                        }
                        else if (item.ComboId.HasValue)
                        {
                            var dr = await _inventoryService.RestoreDefectComboStockAsync(item.ComboId.Value, damagedQty);
                            if (!dr.Succeeded) { await transaction.RollbackAsync(); return Result.Failure(dr.Error!, dr.StatusCode); }

                            var ds = await _inventoryService.DeductComboStockAsync(item.ComboId.Value, damagedQty);
                            if (!ds.Succeeded) { await transaction.RollbackAsync(); return Result.Failure(ds.Error!, ds.StatusCode); }
                        }
                    }
                }

                // Save Evidences (if damaged)
                if (isDamaged && request.EvidenceUrls != null && request.EvidenceUrls.Any())
                {
                    foreach (var url in request.EvidenceUrls)
                    {
                        var evidence = new ShippingEvidence
                        {
                            EvidenceId = Guid.NewGuid(),
                            ShippingTaskId = task.ShippingTaskId,
                            EvidenceUrl = url,
                            // Could store the damage note inside the evidence or task. Let's make it EvidenceType = "ExchangeReport".
                            EvidenceType = "ExchangeReport",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _evidenceRepository.CreateAsync(evidence);
                    }
                }

                // Create new Replacement Task
                var newShippingTask = new ShippingTask
                {
                    ShippingTaskId = Guid.NewGuid(),
                    OrderId = order.Id,
                    StaffId = request.NewStaffId,
                    Status = ShippingTaskStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                await _taskRepository.CreateAsync(newShippingTask);

                await transaction.CommitAsync();
                return Result.Success("Exchange processed successfully. Replacement task created.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result.Failure("Failed to process exchange order.", 500);
            }
        }

        public async Task<Result<ShippingTaskResponse>> GetTaskByIdAsync(Guid taskId)
        {
            var task = await _taskRepository.GetTaskWithDetailsAsync(taskId);
            if (task == null)
            {
                return Result<ShippingTaskResponse>.Failure("Task not found.", 404);
            }
            return Result<ShippingTaskResponse>.Success(MapToResponse(task));
        }

        public async Task<Result<PaginatedList<ShippingTaskResponse>>> GetAllTasksForAdminAsync(int pageNumber, string? status = null, Guid? orderId = null)
        {
            var data = await _taskRepository.GetAllTasksForAdminAsync(pageNumber, status, orderId);

            var responses = data.Items.Select(MapToResponse).ToList();

            return Result<PaginatedList<ShippingTaskResponse>>.Success(
                new PaginatedList<ShippingTaskResponse>(
                    responses, data.TotalCount, data.PageNumber, data.PageSize));
        }

        public async Task<Result<PaginatedList<ShippingTaskResponse>>> GetTasksForStaffAsync(Guid staffId, int pageNumber = 1)
        {
            var data = await _taskRepository.GetTasksByStaffIdAsync(staffId, pageNumber);

            var responses = data.Items.Select(MapToResponse).ToList();

            return Result<PaginatedList<ShippingTaskResponse>>.Success(
                new PaginatedList<ShippingTaskResponse>(
                    responses, data.TotalCount, data.PageNumber, data.PageSize));
        }

        private static ShippingTaskResponse MapToResponse(ShippingTask task)
        {
            return new ShippingTaskResponse
            {
                ShippingTaskId = task.ShippingTaskId,
                StaffId = task.StaffId,
                OrderId = task.OrderId,
                StaffName = task.Staff?.FullName ?? string.Empty,
                OrderCode = task.Order?.OrderCode ?? string.Empty,
                Status = task.Status,
                ShippingDate = task.ShippingDate,
                CompletionDate = task.CompletionDate,
                StaffNote = task.StaffNote,
                Evidences = task.ShippingEvidences?.Select(e => new ShippingEvidenceResponse
                {
                    EvidenceId = e.EvidenceId,
                    EvidenceUrl = e.EvidenceUrl,
                    EvidenceType = e.EvidenceType,
                    CreatedAt = e.CreatedAt
                }).ToList() ?? new List<ShippingEvidenceResponse>()
            };
        }
    }
}
