using System.Text.Json;
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
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = _consumer.Consume(cancellationToken);
            if (result.Message.Value == null)
            {
                _logger.LogInformation("Received null message value, skipping.");
                _consumer.Commit(result);
                continue;
            }

            try
            {
                var message = JsonSerializer.Deserialize<IActivitiesMessage>(result.Message.Value);
                if (message == null)
                {
                    _logger.LogInformation("Deserialized message is null, skipping.");
                    _consumer.Commit(result);
                    continue;
                }
                _logger.LogInformation($"Received message: {message.NewIssue.Title} - {message.NewIssue.Id}");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var issueUseCase = scope.ServiceProvider.GetRequiredService<IssueUseCase>();
                    await handleActivitiesMessage(issueUseCase, message);
                }

                _consumer.Commit(result);
            }
            catch (JsonException ex)
            {
                _logger.LogInformation($"Failed to deserialize message: {ex.Message}");
                _consumer.Commit(result); // Commit to avoid reprocessing
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Close();
        return Task.CompletedTask;
    }

    public void Dispose() => _consumer.Dispose();

    private async Task handleActivitiesMessage(IssueUseCase issueUseCase, IActivitiesMessage message)
    {
        switch (message.EventType)
        {
            case ActivitiesMessageAction.ISSUE_CREATED:
            case ActivitiesMessageAction.ISSUE_CHANGED:
                await issueUseCase.OnIssueChanged(message);
                break;
            default:
                break;
        }
    }
}