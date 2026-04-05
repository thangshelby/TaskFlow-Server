using Confluent.Kafka;
using System.Text.Json;
using System.Text.Json.Serialization;


public class KafkaPublisher : IPublisherService, IDisposable
{
    private readonly IProducer<Null, string> _producer;

    private readonly ILogger<KafkaPublisher> _logger;


    public KafkaPublisher(IConfiguration configuration, ILogger<KafkaPublisher> logger)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration.GetValue("KafkaHost", "localhost:9092"),
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task EmitKafka<T>(TopicName topic, KafkaMessageAction type, T message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        try
        {
            var payload = new KafkaMessage<T>
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
            await _producer.ProduceAsync(topic.ToString(), new Message<Null, string> { Value = json });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending message to topic {topic}");
        }
    }

    public void Dispose() => _producer.Dispose();
}
