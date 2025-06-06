using Confluent.Kafka;
using System.Text.Json;

public class NotificationPublisher : IPublisherService, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic;
    private readonly ILogger<NotificationPublisher> _logger;

    public NotificationPublisher(IConfiguration configuration, ILogger<NotificationPublisher> logger)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration.GetValue("KafkaHost", "localhost:9092"),
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
        _topic = "notifications"; // Match ActivitiesPublisher's hardcoded topic pattern
        _logger = logger;
    }

    public async Task Emit<NotificationMessageDomain>(NotificationMessageDomain message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        try
        {
            var json = JsonSerializer.Serialize(message);
            await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = json });
            _logger.LogInformation($"Published notification: {json}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending notification to topic {_topic}: {ex.Message}");
            // Don't throw - match Activities behavior
        }
    }

    public void Dispose() => _producer.Dispose();
}