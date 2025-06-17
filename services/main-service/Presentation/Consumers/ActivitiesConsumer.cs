using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using MainService.Domain.UseCases;

public class ActivitiesConsumer : IHostedService, IDisposable
{
    private readonly IConsumer<Null, string> _consumer;
    private readonly string _topic;
    private readonly ILogger<ActivitiesConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    public ActivitiesConsumer(IConfiguration configuration, ILogger<ActivitiesConsumer> logger, IServiceProvider serviceProvider)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration.GetValue("KafkaHost", "localhost:9092"),
            GroupId = "activities-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        _consumer = new ConsumerBuilder<Null, string>(config).Build();
        _topic = configuration.GetValue("ActivitiesTopicName", "activities");
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ActivitiesConsumer is listening...");
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
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                        Converters = { new JsonStringEnumConverter(null, allowIntegerValues: false) }
                    };

                    var message = JsonSerializer.Deserialize<KafkaMessage<IActivitiesMessage>>(result.Message.Value, options);

                    if (message == null)
                    {
                        _logger.LogWarning("Deserialized message is null.");
                        _consumer.Commit(result);
                        continue;
                    }

                    _logger.LogInformation($"Received message: {message.Data.NewIssue.Title}");

                    using var scope = _serviceProvider.CreateScope();
                    var issueUseCase = scope.ServiceProvider.GetRequiredService<IssueUseCase>();
                    await handleActivitiesMessage(issueUseCase, message);

                    _consumer.Commit(result);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to deserialize Kafka message");
                    _consumer.Commit(result);
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
        _consumer.Close();
        return Task.CompletedTask;
    }

    public void Dispose() => _consumer.Dispose();

    private async Task handleActivitiesMessage(IssueUseCase issueUseCase, KafkaMessage<IActivitiesMessage> message)
    {
        if (Enum.TryParse<KafkaMessageAction>(message.EventType, out var action))
        {
            switch (action)
            {
                case KafkaMessageAction.ACTIVITIES_ISSUE_CHANGED:
                case KafkaMessageAction.ACTIVITIES_ISSUE_CREATED:
                    await issueUseCase.OnIssueChanged(message);
                    break;
                default:
                    break;
            }
        }
    }
}