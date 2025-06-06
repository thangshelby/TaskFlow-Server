using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Domain.Enums;
using MainService.Domain.Common;

namespace MainService.Domain.UseCases;

public class NotificationUseCase
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IPublisherService _publisher;

    public NotificationUseCase(INotificationRepository notificationRepository, IPublisherService publisher)
    {
        _notificationRepository = notificationRepository;
        _publisher = publisher;
    }

    public async Task<PaginatedResult<NotificationMessageDomain>> GetUserNotifications(string userId, int limit = 20, int offset = 0)
    {
        return await _notificationRepository.GetUserNotifications(userId, limit, offset);
    }

    public async Task<NotificationMessageDomain> CreateIssueAssignedNotification(string userId, string issueTitle, string issueId)
    {
        var notification = new NotificationMessageDomain
        {
            UserId = userId,
            Title = "New Issue Assignment",
            Content = $"You have been assigned to issue: {issueTitle}",
            Type = NotificationType.IssueAssigned,
            ReferenceId = issueId
        };

        return await _notificationRepository.Create(notification);
    }

    public async Task<NotificationMessageDomain> CreateSprintStartingNotification(string userId, string sprintName, string sprintId)
    {
        var notification = new NotificationMessageDomain
        {
            UserId = userId,
            Title = "Sprint Starting",
            Content = $"Sprint '{sprintName}' is starting",
            Type = NotificationType.SprintStarting,
            ReferenceId = sprintId
        };

        return await _notificationRepository.Create(notification);
    }

    public async Task<NotificationMessageDomain> CreateMentionNotification(string userId, string commenterName, string issueId)
    {
        var notification = new NotificationMessageDomain
        {
            UserId = userId,
            Title = "New Mention",
            Content = $"{commenterName} mentioned you in a comment",
            Type = NotificationType.CommentMention,
            ReferenceId = issueId
        };

        return await _notificationRepository.Create(notification);
    }

    public async Task<NotificationMessageDomain> CreateProjectInvitationNotification(string userId, string projectName, string projectId)
    {
        var notification = new NotificationMessageDomain
        {
            UserId = userId,
            Title = "Project Invitation",
            Content = $"You have been invited to join project: {projectName}",
            Type = NotificationType.ProjectInvitation,
            ReferenceId = projectId
        };

        return await _notificationRepository.Create(notification);
    }

    public async Task MarkAsRead(string notificationId)
    {
        await _notificationRepository.MarkAsRead(notificationId);
    }

    public async Task MarkAllAsRead(string userId)
    {
        await _notificationRepository.MarkAllAsRead(userId);
    }

    public async Task<List<NotificationMessageDomain>> GetUnreadNotifications(string userId)
    {
        return await _notificationRepository.GetUnreadNotifications(userId);
    }

    public async Task<int> GetUnreadCount(string userId)
    {
        return await _notificationRepository.GetUnreadCount(userId);
    }
}