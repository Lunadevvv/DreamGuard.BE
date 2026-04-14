using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Implements;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubService _hubService;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(INotificationRepository notificationRepository, IHubService hubService, IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            _hubService = hubService;
            _unitOfWork = unitOfWork;
        }
        public async Task SendNotificationAsync(Notification notification)
        {
           await _notificationRepository.CreateAsync(notification);
           await _hubService.SendNotificationToStaff(notification);
        }


        public async Task<Result<PaginatedList<Notification>>> GetMyNotificationAsync(Guid userId, int pageNumber, int pageSize)
        {
            var notifications = await _notificationRepository.GetMyNotificationAsync(userId, pageNumber, pageSize);
            return Result<PaginatedList<Notification>>.Success(notifications);
        }

        public async Task<Result> MarkAsReadAsync(Guid userId)
        {
            var notification = await _notificationRepository.GetByUserIdAsync(userId);
            if (notification.Any())
            {
                foreach (var item in notification)
                {
                    item.IsRead = true;
                    _notificationRepository.UpdateEntity(item);
                }
                await _unitOfWork.SaveChangeAsync();
            }
            return Result.Success("All notifications marked as read.");
        }
    }
}
