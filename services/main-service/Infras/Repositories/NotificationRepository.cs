using AutoMapper;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Domain.Common;
using MainService.Infras.Entities;
using MongoDB.Driver;

namespace MainService.Infras.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ILogger<NotificationRepository> _logger;
    private readonly IMongoCollection<NotificationEntity> _notifications;
    private readonly IMapper _mapper;

    public NotificationRepository(MongoDbService mongoDbService, IMapper mapper, ILogger<NotificationRepository> logger)
    {
        _logger = logger;
        _mapper = mapper;
        var database = mongoDbService.Database;
        _notifications = database.GetCollection<NotificationEntity>("notifications");
    }

    public async Task<NotificationMessageDomain> Create(NotificationMessageDomain notification)
    {
        try
        {
            var entity = NotificationEntity.FromDomain(notification);
            await _notifications.InsertOneAsync(entity);
            notification.Id = entity.Id;
            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert notification");
            throw;
        }
    }

    public async Task<PaginatedResult<NotificationMessageDomain>> GetUserNotifications(string userId, int limit = 20, int offset = 0)
    {
        try
        {
            var filter = Builders<NotificationEntity>.Filter.Eq(n => n.UserId, userId);
            var totalCount = await _notifications.CountDocumentsAsync(filter);

            var entities = await _notifications
                .Find(filter)
                .SortByDescending(n => n.CreatedAt)
                .Skip(offset)
                .Limit(limit)
                .ToListAsync();

            var items = _mapper.Map<List<NotificationMessageDomain>>(entities);
            return new PaginatedResult<NotificationMessageDomain>(items, (int)totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user notifications");
            throw;
        }
    }

    public async Task<NotificationMessageDomain> MarkAsRead(string notificationId)
    {
        try
        {
            var filter = Builders<NotificationEntity>.Filter.Eq(n => n.Id, notificationId);
            var update = Builders<NotificationEntity>.Update
                .Set(n => n.IsRead, true)
                .Set(n => n.UpdatedAt, DateTime.UtcNow);

            var entity = await _notifications.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<NotificationEntity> { ReturnDocument = ReturnDocument.After }
            );

            if (entity == null)
                throw new Exception("Notification not found");

            return _mapper.Map<NotificationMessageDomain>(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notification as read");
            throw;
        }
    }

    public async Task<bool> Delete(string notificationId)
    {
        try
        {
            var result = await _notifications.DeleteOneAsync(n => n.Id == notificationId);
            return result.DeletedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete notification");
            throw;
        }
    }

    public async Task<List<NotificationMessageDomain>> GetUnreadNotifications(string userId)
    {
        try
        {
            var filter = Builders<NotificationEntity>.Filter.And(
                Builders<NotificationEntity>.Filter.Eq(n => n.UserId, userId),
                Builders<NotificationEntity>.Filter.Eq(n => n.IsRead, false)
            );

            var entities = await _notifications
                .Find(filter)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<NotificationMessageDomain>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unread notifications");
            throw;
        }
    }

    public async Task<int> GetUnreadCount(string userId)
    {
        try
        {
            var filter = Builders<NotificationEntity>.Filter.And(
                Builders<NotificationEntity>.Filter.Eq(n => n.UserId, userId),
                Builders<NotificationEntity>.Filter.Eq(n => n.IsRead, false)
            );

            return (int)await _notifications.CountDocumentsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unread notification count");
            throw;
        }
    }

    public async Task MarkAllAsRead(string userId)
    {
        try
        {
            var filter = Builders<NotificationEntity>.Filter.And(
                Builders<NotificationEntity>.Filter.Eq(n => n.UserId, userId),
                Builders<NotificationEntity>.Filter.Eq(n => n.IsRead, false)
            );

            var update = Builders<NotificationEntity>.Update
                .Set(n => n.IsRead, true)
                .Set(n => n.UpdatedAt, DateTime.UtcNow);

            await _notifications.UpdateManyAsync(filter, update);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark all notifications as read");
            throw;
        }
    }
}