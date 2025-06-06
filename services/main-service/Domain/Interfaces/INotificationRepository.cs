using MainService.Domain.Entities;
using MainService.Domain.Common;

namespace MainService.Domain.Interfaces;

public interface INotificationRepository
{
    Task<NotificationMessageDomain> Create(NotificationMessageDomain notification);
    Task<PaginatedResult<NotificationMessageDomain>> GetUserNotifications(string userId, int limit = 20, int offset = 0);
    Task<NotificationMessageDomain> MarkAsRead(string notificationId);
    Task<bool> Delete(string notificationId);
    Task<List<NotificationMessageDomain>> GetUnreadNotifications(string userId);
    Task<int> GetUnreadCount(string userId);
    Task MarkAllAsRead(string userId);
}