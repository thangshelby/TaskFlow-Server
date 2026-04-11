using System;
using MainService.Domain.Interfaces;
using System.Linq;
using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS.Model;
using Amazon.SQS;
using SnsMessageAttributeValue = Amazon.SimpleNotificationService.Model.MessageAttributeValue;

public class AWSQueueRepository: IQueueRepository {
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly IAmazonSQS _sqsClient;
    private readonly ILogger<AWSQueueRepository> _logger;
    private readonly IConfiguration _configuration;

    private readonly string _topicArn = string.Empty;
    private string _queueName = string.Empty;
    private string? _queueUrl;

    public AWSQueueRepository(IAmazonSimpleNotificationService snsClient, IAmazonSQS sqsClient, ILogger<AWSQueueRepository> logger, IConfiguration configuration)
    {
        _snsClient = snsClient;
        _sqsClient = sqsClient;
        _logger = logger;
        _configuration = configuration;
        _topicArn = _configuration.GetValue<string>("AWS:SNS:TopicArn") ?? string.Empty;
        _queueName = _configuration.GetValue<string>("AWS:SQS:QueueName") ?? string.Empty;
    }

    public async Task<bool> SendMessage(QueueTopicName topicName, QueueMessageAction action, string message){
        _logger.LogInformation(
            "Sending message to SNS. TopicArn={TopicArn}, Action={Action}, TopicName={TopicName}, MessageLength={MessageLength}",
            _topicArn,
            action,
            topicName,
            message.Length);
        try {
            var request = new PublishRequest 
            {
                TopicArn = _topicArn,
                Message = message,
                Subject = action.ToString(),
                MessageGroupId = topicName.ToString(),
                MessageDeduplicationId = Guid.NewGuid().ToString(),
                MessageAttributes = new Dictionary<string, SnsMessageAttributeValue>
                {
                    { "Action", new SnsMessageAttributeValue { DataType = "String", StringValue = action.ToString() } },
                    { "Topic", new SnsMessageAttributeValue { DataType = "String", StringValue = topicName.ToString() } }
                }
            };

            
            await _snsClient.PublishAsync(request);
            return true;
        } catch (Exception ex) {
            _logger.LogError(ex, "Error sending message to SNS");
            return false;
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken, string topic)
    {
        _queueUrl = await ResolveQueueUrl(topic);
        if (string.IsNullOrWhiteSpace(_queueUrl))
            throw new InvalidOperationException("SQS QueueUrl is empty. Check AWS:SQS:QueueUrl (or topic-specific keys).");
    }

    public async Task<QueueReceivedMessage?> ReceiveMessage()
    {
        EnsureQueueUrlConfigured();

        var receiveRequest = new ReceiveMessageRequest
        {
            QueueUrl = _queueUrl,
            MaxNumberOfMessages = 1,
            WaitTimeSeconds = 10
        };
        var messageResponse = await _sqsClient.ReceiveMessageAsync(receiveRequest);

        if (messageResponse.Messages?.Count > 0)
        {
            var message = messageResponse.Messages[0];
            return new QueueReceivedMessage
            {
                Payload = TryUnwrapQueueEnvelopePayload(message.Body ?? string.Empty),
                ReceiptHandle = message.ReceiptHandle
            };
        }

        return null;
    }

  

    public async Task Commit(string? receiptHandle)
    {
        EnsureQueueUrlConfigured();
        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            _logger.LogDebug("No SQS receipt handle available to commit");
            return;
        }

        try
        {
            var deleteMessageRequest = new DeleteMessageRequest
            {
                QueueUrl = _queueUrl,
                ReceiptHandle = receiptHandle,
            };

            await _sqsClient.DeleteMessageAsync(deleteMessageRequest);
            _logger.LogInformation("Message deleted from SQS");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message from SQS");
            throw;
        }
    }

    public void Dispose()
    {
        _snsClient.Dispose();
        _sqsClient.Dispose();
    }
    private async Task<string> ResolveQueueUrl(string topic)
    {
        var response = await _sqsClient.GetQueueUrlAsync(_queueName);
        if (response.QueueUrl != null)
            return response.QueueUrl;

        return string.Empty;
    }

    private void EnsureQueueUrlConfigured()
    {
        if (string.IsNullOrWhiteSpace(_queueUrl))
            throw new InvalidOperationException("SQS QueueUrl is empty. Check AWS:SQS:QueueUrl (or topic-specific keys).");
    }

    private static string TryUnwrapQueueEnvelopePayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return payload;

        // Defensive: if SQS contains an SNS notification envelope, unwrap it.
        // AWS common shape: { "Type": "Notification", "Message": "<original payload json>", ... }
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return payload;

            if (doc.RootElement.TryGetProperty("Message", out var messageProp) &&
                messageProp.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(messageProp.GetString()))
            {
                return messageProp.GetString()!;
            }

            if (doc.RootElement.TryGetProperty("message", out var messageProp2) &&
                messageProp2.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(messageProp2.GetString()))
            {
                return messageProp2.GetString()!;
            }
        }
        catch (JsonException)
        {
            // Not an envelope; return raw payload.
        }

        return payload;
    }
}