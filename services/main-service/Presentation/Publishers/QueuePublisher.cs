using System.Text.Json;
using System.Text.Json.Serialization;
using MainService.Domain.Interfaces;


public class QueuePublisher : IPublisherService
{
    private readonly ILogger<QueuePublisher> _logger;

    private readonly IQueueRepository _queueRepository;
    public QueuePublisher(ILogger<QueuePublisher> logger, IQueueRepository queueRepository)
    {
        _queueRepository = queueRepository;
        _logger = logger;
    }

    public async Task EmitQueue<T>(QueueTopicName topic, QueueMessageAction type, T message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        _logger.LogInformation("Emitting message to topic {Topic} with action {Action} and message {Message}", topic, type, message);
        try
        {
            var payload = new QueueMessage<T>
            {
                EventType = type.ToString(),
                Data = message
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
            });

            await _queueRepository.SendMessage(topic, type, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending message to topic {topic}");
        }
    }
}
