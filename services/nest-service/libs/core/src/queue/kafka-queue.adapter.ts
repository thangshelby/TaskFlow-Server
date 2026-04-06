import { Injectable } from '@nestjs/common';
import { EachMessagePayload } from 'kafkajs';
import { KafkaService } from './kafka.service';
import { IQueueService, QueueMessageHandler } from './queue.interface';

@Injectable()
export class KafkaQueueAdapter implements IQueueService {
  constructor(private readonly kafkaService: KafkaService) {}

  async subscribe(queueName: string, callback: QueueMessageHandler): Promise<void> {
    await this.kafkaService.subscribe(queueName, async (payload: EachMessagePayload) => {
      const value = payload.message.value?.toString();
      if (!value) return;
      await callback(value);
    });
  }
}

