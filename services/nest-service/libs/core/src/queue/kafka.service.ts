import { Injectable, OnModuleInit, OnModuleDestroy } from '@nestjs/common';
import { Kafka, Producer, Consumer, EachMessagePayload, Message, Partitioners } from 'kafkajs';
import { v4 as uuidv4 } from 'uuid';
import { LogService } from '../log/log.service';
import { ConfigService } from '@nestjs/config';

export interface KafkaMessage {
  id: string;
  eventType: string;
  data?: Record<string, unknown>;
  [key: string]: unknown;
}

@Injectable()
export class KafkaService implements OnModuleInit, OnModuleDestroy {
  private readonly kafka: Kafka;
  private producer: Producer;
  private consumer: Consumer;

  constructor(private readonly logService: LogService, private readonly configService: ConfigService) {
    const clientId = this.configService.get<string>('KAFKA_CLIENT_ID', 'taskflow-client');
    const brokers = this.configService.get<string>('KAFKA_BROKERS', 'localhost:9092').split(',');
    const groupId = this.configService.get<string>('KAFKA_GROUP_ID', 'taskflow-group');

    this.kafka = new Kafka({
      clientId,
      brokers,
    });
    this.producer = this.kafka.producer({
      createPartitioner: Partitioners.LegacyPartitioner,
    });
    this.consumer = this.kafka.consumer({ groupId });
  }

  async onModuleInit() {
    await this.producer.connect();
    this.logService.log('Kafka Producer connected');
    await this.consumer.connect();
    this.logService.log('Kafka Consumer connected');
  }

  async onModuleDestroy() {
    await this.producer.disconnect();
    await this.consumer.disconnect();
    this.logService.log('Kafka disconnected');
  }

  async publish(topic: string, message: KafkaMessage, config: { partitionKey?: string } = {}): Promise<void> {
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
    await this.consumer.subscribe({ topic, fromBeginning: true });

    await this.consumer.run({
      eachMessage: async (payload) => {
        try {
          await callback(payload);
          this.logService.log(`Processed message from ${topic} [${payload.partition}]: ${payload.message.value?.toString() || ''}`);
        } catch (error: any) {
          this.logService.error(`Error processing message from ${topic}`, error.stack);
        }
      },
    });

    this.logService.log(`Subscribed to topic: ${topic}`);
  }

  on(topic: string, callback: (message: EachMessagePayload) => Promise<void>): void {
    void this.subscribe(topic, callback);
  }
}
