using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using MainService.Domain.UseCases;

public class ActivitiesConsumer : IHostedService, IDisposable
{
    private readonly IQueueRepository _queueRepository;
    private readonly string _topic;
    private readonly ILogger<ActivitiesConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    private IConfiguration _configuration;

    public ActivitiesConsumer(IConfiguration configuration, IQueueRepository queueRepository, ILogger<ActivitiesConsumer> logger, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _queueRepository = queueRepository;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _topic = configuration.GetValue<string>("ActivitiesTopicName");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ActivitiesConsumer is listening...");
        await _queueRepository.StartAsync(cancellationToken, _topic);
        _ = Task.Run(() => ConsumeLoop(cancellationToken), cancellationToken);
    }

    private async Task ConsumeLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var queueMessage = await _queueRepository.ReceiveMessage();
                if (queueMessage == null || string.IsNullOrEmpty(queueMessage.Payload))
                {
                    continue;
                }

                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                        Converters = { new JsonStringEnumConverter(null, allowIntegerValues: false) }
                    };

                    var message = JsonSerializer.Deserialize<QueueMessage<IActivitiesMessage>>(queueMessage.Payload, options);

                    if (message == null)
                    {
                        await _queueRepository.Commit(queueMessage.ReceiptHandle);
                        continue;
                    }


                    using var scope = _serviceProvider.CreateScope();
                    var issueUseCase = scope.ServiceProvider.GetRequiredService<IssueUseCase>();
                    await handleActivitiesMessage(issueUseCase, message);

                    await _queueRepository.Commit(queueMessage.ReceiptHandle);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to deserialize Kafka message");
                    await _queueRepository.Commit(queueMessage.ReceiptHandle);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Kafka consume loop");
        }
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        // await _queueRepository.Commit();
        // return Task.CompletedTask;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _queueRepository.Dispose();
    }

    private async Task handleActivitiesMessage(IssueUseCase issueUseCase, QueueMessage<IActivitiesMessage> message)
    {
        if (Enum.TryParse<QueueMessageAction>(message.EventType, out var action))
        {
            switch (action)
            {
                case QueueMessageAction.ACTIVITIES_ISSUE_CHANGED:
                case QueueMessageAction.ACTIVITIES_ISSUE_CREATED:
                    await issueUseCase.OnIssueChanged(message);
                    break;
                default:
                    break;
            }
        }
    }
}