import { Injectable } from '@nestjs/common';
import { AwsService } from './aws.service';
import { IQueueService, QueueMessageHandler } from './queue.interface';

@Injectable()
export class AwsQueueAdapter implements IQueueService {
  constructor(private readonly awsService: AwsService) {}

  async subscribe(queueName: string, callback: QueueMessageHandler): Promise<void> {
    await this.awsService.subscribe(queueName, async (messageBodyMessage: unknown) => {
      // AwsService passes `messageBody.Message` to callback.
      // Normalize to string so subscribers can do `JSON.parse(...)`.
      const normalized =
        typeof messageBodyMessage === 'string'
          ? messageBodyMessage
          : messageBodyMessage === null || messageBodyMessage === undefined
            ? ''
            : JSON.stringify(messageBodyMessage);

      await callback(normalized);
    });
  }
}

