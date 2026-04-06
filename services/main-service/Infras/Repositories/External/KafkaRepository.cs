using Confluent.Kafka;
using MainService.Domain.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

public class KafkaRepository: IQueueRepository {
    private readonly ILogger<IQueueRepository> _logger;
    private readonly IProducer<Null, string> _producer;
    private readonly IConsumer<Null, string> _consumer;
    public KafkaRepository(IConfiguration configuration, ILogger<IQueueRepository> logger)
    {
        _logger = logger;
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = configuration.GetValue("Kafka:Host", "localhost:9092"),
            Acks = Acks.All
        };

        var consumerConfig = new ConsumerConfig
        {                        
            BootstrapServers = configuration.GetValue("Kafka:Host", "localhost:9092"),
            GroupId = configuration.GetValue("Kafka:GroupId", "activities-group"),
            AutoOffsetReset = AutoOffsetReset.Earliest,   
            EnableAutoCommit = true,
        };
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        _consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
     
    }

    public async Task<bool> SendMessage(QueueTopicName topicName, QueueMessageAction action, string message) {
        try {
           await _producer.ProduceAsync(topicName.ToString(), new Message<Null, string> { Value = message });
           return true;
        } catch (Exception ex) {
            _logger.LogError(ex, "Error sending message to Kafka");
            return false;
        }
    }

    public Task StartAsync(CancellationToken cancellationToken, string topic)
    {
        _logger.LogInformation("KafkaRepository is listening...");
        _consumer.Subscribe(topic);
        return Task.CompletedTask;
    }

    public Task<QueueReceivedMessage?> ReceiveMessage()
    {
        var result = _consumer.Consume();
        if (result?.Message?.Value == null)
            return Task.FromResult<QueueReceivedMessage?>(null);

        var commitToken = JsonSerializer.Serialize(new KafkaCommitToken
        {
            Topic = result.Topic,
            Partition = result.Partition.Value,
            Offset = result.Offset.Value
        });

        return Task.FromResult<QueueReceivedMessage?>(new QueueReceivedMessage
        {
            Payload = result.Message.Value,
            ReceiptHandle = commitToken
        });
    }

    public Task Commit(string? receiptHandle)
    {
        if (string.IsNullOrWhiteSpace(receiptHandle))
            return Task.CompletedTask;

        try
        {
            var token = JsonSerializer.Deserialize<KafkaCommitToken>(receiptHandle);
            if (token == null || string.IsNullOrWhiteSpace(token.Topic))
                return Task.CompletedTask;

            var topicPartitionOffset = new TopicPartitionOffset(
                new TopicPartition(token.Topic, new Partition(token.Partition)),
                new Offset(token.Offset + 1));

            _consumer.Commit(new[] { topicPartitionOffset });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid Kafka commit token");
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _consumer.Dispose();
        _producer.Dispose();
    }

    private sealed class KafkaCommitToken
    {
        [JsonPropertyName("topic")]
        public string Topic { get; init; } = string.Empty;

        [JsonPropertyName("partition")]
        public int Partition { get; init; }

        [JsonPropertyName("offset")]
        public long Offset { get; init; }
    }
}