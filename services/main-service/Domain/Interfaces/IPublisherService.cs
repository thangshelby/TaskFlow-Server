

public interface IPublisherService
{
    Task Emit<T>(T message);
}