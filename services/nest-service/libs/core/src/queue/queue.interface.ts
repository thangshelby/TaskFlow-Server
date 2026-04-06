export type QueueMessageHandler = (message: string) => Promise<void>;

/**
 * Transport-agnostic queue consumer abstraction.
 * - For Kafka: queueName is `topic`
 * - For AWS SQS: queueName is `queueUrl`
 */
export interface IQueueService {
  subscribe(queueName: string, callback: QueueMessageHandler): Promise<void>;
}

/**
 * Nest injection token for DI.
 * Use `@Inject(QUEUE_SERVICE_TOKEN)` in consumers.
 */
export const QUEUE_SERVICE_TOKEN = 'QUEUE_SERVICE_TOKEN';

