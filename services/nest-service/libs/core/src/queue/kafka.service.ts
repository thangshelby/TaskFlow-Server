import { Injectable, OnModuleDestroy } from '@nestjs/common';
import { Kafka, Producer, Consumer, EachMessagePayload, Message, Partitioners } from 'kafkajs';
import { v4 as uuidv4 } from 'uuid';
import { LogService } from '../log/log.service';
import { ConfigService } from '@nestjs/config';

export interface KafkaMessage {
  id: string;
  eventType: KafkaActionType;
  data?: Record<string, unknown>;
  [key: string]: unknown;
}

export enum KafkaActionType {
  NOTIFICATIONS_CREATE_NEW_NOTIFICATION = 'NOTIFICATIONS_CREATE_NEW_NOTIFICATION',
  MAILS_SEND_VERIFY_OTP_USER = 'MAILS_SEND_VERIFY_OTP_USER',
}

@Injectable()
export class KafkaService implements OnModuleDestroy {
  private readonly kafka: Kafka;
  private producer: Producer;
  private consumers: Consumer[] = [];
  private producerConnected = false;

  constructor(
    private readonly logService: LogService,
    private readonly configService: ConfigService,
  ) {
    const clientId = this.configService.get<string>('KAFKA_CLIENT_ID', 'taskflow-client');
    const brokers = this.configService.get<string>('KAFKA_BROKERS', 'localhost:29092').split(',');

    this.kafka = new Kafka({
      clientId,
      brokers,
    });
    this.producer = this.kafka.producer({
      createPartitioner: Partitioners.LegacyPartitioner,
    });
  }

  private async ensureProducerConnected(): Promise<void> {
    if (this.producerConnected) {
      return;
    }
    await this.producer.connect();
    this.producerConnected = true;
    this.logService.log('Kafka Producer connected');
  }

  async onModuleDestroy() {
    for (const consumer of this.consumers) {
      await consumer.disconnect();
    }
    if (this.producerConnected) {
      await this.producer.disconnect();
    }
    this.logService.log('Kafka disconnected');
  }

  async publish(topic: string, message: KafkaMessage, config: { partitionKey?: string } = {}): Promise<void> {
    await this.ensureProducerConnected();
    try {
      const kafkaMessage: Message = {
        value: JSON.stringify(message),
        key: config.partitionKey ?? `part_${uuidv4()}`,
      };

      await this.producer.send({
        topic,
        messages: [kafkaMessage],
      });

      this.logService.log(`Message published to topic ${topic}: ${JSON.stringify(message)}`);
    } catch (error: any) {
      this.logService.error(`Failed to publish message to ${topic}`, error.stack);
      throw error;
    }
  }

  async emit(topic: string, message: KafkaMessage, config?: { partitionKey?: string }): Promise<void> {
    if (!config && !('id' in message)) {
      throw new Error('Message id is required');
    }
    await this.publish(topic, message, config);
  }

  async subscribe(topic: string, callback: (message: EachMessagePayload) => Promise<void>): Promise<void> {
    const baseGroupId = this.configService.get<string>('KAFKA_GROUP_ID', 'taskflow-group');
    const consumer = this.kafka.consumer({ groupId: `${baseGroupId}-${topic}` });

    await consumer.connect();
    await consumer.subscribe({ topic, fromBeginning: true });

    await consumer.run({
      eachMessage: async (payload) => {
        try {
          await callback(payload);
          this.logService.log(`Processed message from ${topic} [${payload.partition}]: ${payload.message.value?.toString() || ''}`);
        } catch (error: any) {
          this.logService.error(`Error processing message from ${topic}`, error.stack);
        }
      },
    });

    this.consumers.push(consumer);
    this.logService.log(`Subscribed to topic: ${topic}`);
  }

  on(topic: string, callback: (message: EachMessagePayload) => Promise<void>): void {
    void this.subscribe(topic, callback);
  }
}
