import { Service } from 'typedi';
import { Consumer, EachMessagePayload, Kafka, Message, Producer } from 'kafkajs';
import { v4 as uuidv4 } from 'uuid';
import { NotificationActionType } from 'src/common/actions/notification.action';

export interface KafkaMessage {
  id: string;
  eventType: NotificationActionType;
  data?: Record<string, unknown>;
  [key: string]: unknown;
}
@Service()
export class KafkaService {
  private client: Kafka;
  private producer: Producer;
  private consumer: Consumer;

  constructor() {
    this.client = new Kafka({
      clientId: 'myApp',
      brokers: ['localhost:9092'],
    });

    this.producer = this.client.producer();
    this.consumer = this.client.consumer({ groupId: 'default-group' });
  }

  async connect(): Promise<void> {
    await this.producer.connect();
    console.log('Kafka producer connected');
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

      console.log('Message is published: topic=%s, message=%o', topic, message);
    } catch (err) {
      console.log('Message publishing failed: topic=%s, message=%o, error=%o', topic, message, err);
      throw err;
    }
  }

  async subscribe(topic: string, callback: (message: EachMessagePayload) => Promise<void>): Promise<void> {
    await this.consumer.subscribe({ topic, fromBeginning: true });

    await this.consumer.run({
      eachMessage: async (payload) => {
        try {
          await callback(payload);
        } catch (err) {
          console.error('Error processing message:', err);
        }
      },
    });

    console.log(`Subscribed to topic: ${topic}`);
  }

  async emit(topic: string, message: KafkaMessage, config?: { partitionKey?: string }): Promise<void> {
    if (!config && !('id' in message)) throw new Error('Message id is required');

    await this.publish(topic, message, config);
  }

  on(topic: string, cb: (message: EachMessagePayload) => Promise<void>): void {
    void this.subscribe(topic, cb);
  }

  async stop(): Promise<void> {
    await this.producer.disconnect();
    console.log('Kafka producer disconnected');
  }
}
