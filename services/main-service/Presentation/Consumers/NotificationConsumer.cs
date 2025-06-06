using System.Text.Json;
using Confluent.Kafka;
using MainService.Domain.Entities;
using MainService.Domain.UseCases;
using MainService.Domain.Enums;

public class NotificationConsumer : IHostedService, IDisposable
{
    private readonly IConsumer<Null, string> _consumer;
    private readonly string _topic;
    private readonly ILogger<NotificationConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public NotificationConsumer(IConfiguration configuration, ILogger<NotificationConsumer> logger, IServiceProvider serviceProvider)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration.GetValue("KafkaHost", "localhost:9092"),
            GroupId = "notifications-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        _consumer = new ConsumerBuilder<Null, string>(config).Build();
        _topic = "notifications"; // Match Activity pattern of using hardcoded topic name
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NotificationConsumer is listening...");
        _consumer.Subscribe(_topic);
        Task.Run(() => ConsumeLoop(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    private async Task ConsumeLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(cancellationToken);
                if (result?.Message?.Value == null)
                {
                    _logger.LogWarning("Received null message.");
                    _consumer.Commit(result);
                    continue;
                }

                try
                {
                    var notification = JsonSerializer.Deserialize<NotificationMessageDomain>(result.Message.Value);

                    if (notification == null)
                    {
                        _logger.LogWarning("Deserialized notification is null.");
                        _consumer.Commit(result);
                        continue;
                    }

                    _logger.LogInformation($"Received notification: {notification.Title}");

                    using var scope = _serviceProvider.CreateScope();
                    var notificationUseCase = scope.ServiceProvider.GetRequiredService<NotificationUseCase>();
                    await HandleNotification(notificationUseCase, notification);

                    _consumer.Commit(result);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to deserialize notification message");
                    _consumer.Commit(result);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation, no need to log error
            _logger.LogInformation("Notification consumer stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in notification consume loop");
        }
    }

    private async Task HandleNotification(NotificationUseCase notificationUseCase, NotificationMessageDomain notification)
    {
        try
        {
            var result = notification.Type switch
            {
                NotificationType.IssueAssigned => await notificationUseCase.CreateIssueAssignedNotification(
                    notification.UserId, notification.Title, notification.ReferenceId),
                
                NotificationType.SprintStarting => await notificationUseCase.CreateSprintStartingNotification(
                    notification.UserId, notification.Title, notification.ReferenceId),
                
                NotificationType.CommentMention => await notificationUseCase.CreateMentionNotification(
                    notification.UserId, notification.Title, notification.ReferenceId),
                
                NotificationType.ProjectInvitation => await notificationUseCase.CreateProjectInvitationNotification(
                    notification.UserId, notification.Title, notification.ReferenceId),
                
                _ => throw new ArgumentException($"Unsupported notification type: {notification.Type}")
            };

            _logger.LogInformation($"Processed notification {result.Id} for user {notification.UserId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling notification");
            // Don't rethrow - we want to continue processing other messages
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Close();
        return Task.CompletedTask;
    }

    public void Dispose() => _consumer.Dispose();
}