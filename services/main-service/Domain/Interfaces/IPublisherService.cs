using MainService.Domain.Entities;
namespace MainService.Domain.Interfaces;

public interface IPublisherService
{
    Task EmitQueue<T>(QueueTopicName topic, QueueMessageAction type, T message);
}