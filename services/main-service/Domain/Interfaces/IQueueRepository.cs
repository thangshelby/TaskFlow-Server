public interface IQueueRepository {
    Task<bool> SendMessage(QueueTopicName topicName, QueueMessageAction action, string message);

    Task StartAsync(CancellationToken cancellationToken, string topic);
    Task<QueueReceivedMessage?> ReceiveMessage();
    Task Commit(string? receiptHandle);
    void Dispose();
}

public sealed class QueueReceivedMessage
{
    public string Payload { get; init; } = string.Empty;
    public string? ReceiptHandle { get; init; }
}