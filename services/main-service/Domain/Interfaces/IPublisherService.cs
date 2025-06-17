

public interface IPublisherService
{
    Task EmitKafka<T>(TopicName topic, KafkaMessageAction type, T message);
}