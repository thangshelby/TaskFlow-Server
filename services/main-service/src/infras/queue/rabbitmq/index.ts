import amqplib from 'amqplib';
import { IMessageQueue } from '~/domain/interfaces/queue';

export class RabbitMQService implements IMessageQueue {
  private connection!: amqplib.Connection;
  private channel!: amqplib.Channel;

  async init(): Promise<void> {
    try {
      this.connection = await amqplib.connect('amqp://localhost');
      this.channel = await this.connection.createChannel();
      console.log('✅ RabbitMQ Connected');
    } catch (error) {
      console.error('❌ RabbitMQ Connection Failed:', error);
    }
  }

  async sendMessage(queue: string, message: object): Promise<void> {
    if (!this.channel) throw new Error('RabbitMQ channel not initialized');
    await this.channel.assertQueue(queue, { durable: true });
    this.channel.sendToQueue(queue, Buffer.from(JSON.stringify(message)), {
      persistent: true
    });
  }
}
