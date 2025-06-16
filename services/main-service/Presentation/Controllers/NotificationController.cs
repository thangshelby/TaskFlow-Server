using AutoMapper;
using Grpc.Core;
using MainService.Domain.UseCases;
using TaskFlow.NotificationService;

public class NotificationController : NotificationService.NotificationServiceBase
{
    private readonly NotificationUseCase _notificationUseCase;
    private readonly IMapper _mapper;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        NotificationUseCase notificationUseCase,
        IMapper mapper,
        ILogger<NotificationController> logger)
    {
        _notificationUseCase = notificationUseCase ?? throw new ArgumentNullException(nameof(notificationUseCase));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<GetUserNotificationsResponse> GetUserNotifications(GetUserNotificationsRequest request, ServerCallContext context)
    {
        var userId = context.UserState.ContainsKey("UserId") ? context.UserState["UserId"] as string : null;
        if (string.IsNullOrEmpty(userId))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User must be authenticated"));
        }

        try
        {
            // Calculate offset from page and limit
            int offset = (request.Page - 1) * request.Limit;
            var notifications = await _notificationUseCase.GetUserNotifications(userId, request.Limit, offset);
            var response = new GetUserNotificationsResponse();
            response.Data.AddRange(_mapper.Map<List<BaseService.NotificationRes>>(notifications));
            response.Pagination = new BaseService.PaginationRes
            {
                TotalItems = notifications.TotalCount, // Ensure NotificationUseCase returns TotalCount
                TotalPages = (int)Math.Ceiling((double)notifications.TotalCount / request.Limit),
                CurrentPage = request.Page,
                Limit = request.Limit
            };
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notifications for user {UserId}", userId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to get notifications"));
        }
    }

    public override async Task<GetUnreadNotificationsResponse> GetUnreadNotifications(GetUnreadNotificationsRequest request, ServerCallContext context)
    {
        var userId = context.UserState.ContainsKey("UserId") ? context.UserState["UserId"] as string : null;
        if (string.IsNullOrEmpty(userId))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User must be authenticated"));
        }

        try
        {
            var notifications = await _notificationUseCase.GetUnreadNotifications(userId);
            var response = new GetUnreadNotificationsResponse();
            response.Data.AddRange(_mapper.Map<List<BaseService.NotificationRes>>(notifications));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unread notifications for user {UserId}", userId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to get unread notifications"));
        }
    }

    public override async Task<NotificationResponse> MarkAsRead(MarkAsReadRequest request, ServerCallContext context)
    {
        try
        {
            await _notificationUseCase.MarkAsRead(request.NotificationId);
            var userId = context.UserState.ContainsKey("UserId") ? context.UserState["UserId"] as string : null;
            var notifications = await _notificationUseCase.GetUserNotifications(userId, 1, 0);
            var updatedNotification = notifications.Items.FirstOrDefault(n => n.Id == request.NotificationId);

            if (updatedNotification == null)
            {
                throw new KeyNotFoundException($"Notification {request.NotificationId} not found");
            }

            return new NotificationResponse
            {
                Notification = _mapper.Map<BaseService.NotificationRes>(updatedNotification)
            };
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notification {NotificationId} as read", request.NotificationId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to mark notification as read"));
        }
    }

    public override async Task<MarkAllAsReadResponse> MarkAllAsRead(MarkAllAsReadRequest request, ServerCallContext context)
    {
        var userId = context.UserState.ContainsKey("UserId") ? context.UserState["UserId"] as string : null;
        if (string.IsNullOrEmpty(userId))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User must be authenticated"));
        }

        try
        {
            await _notificationUseCase.MarkAllAsRead(userId);
            return new MarkAllAsReadResponse { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark all notifications as read for user {UserId}", userId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to mark all notifications as read"));
        }
    }

    public override async Task<GetUnreadCountResponse> GetUnreadCount(GetUnreadCountRequest request, ServerCallContext context)
    {
        var userId = context.UserState.ContainsKey("UserId") ? context.UserState["UserId"] as string : null;
        if (string.IsNullOrEmpty(userId))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User must be authenticated"));
        }

        try
        {
            var count = await _notificationUseCase.GetUnreadCount(userId);
            return new GetUnreadCountResponse { Count = count };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unread count for user {UserId}", userId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to get unread count"));
        }
    }
}