import { OnModuleInit } from '@nestjs/common';
import { EachMessagePayload } from 'kafkajs';
import { UserCreatedData } from 'src/models/notification';
import { NotificationAction } from 'src/models/notification.action';
import { KafkaMessage, KafkaService } from 'src/services/kafka.service';
import { NotificationService } from 'src/services/notification.service';
import { Service } from 'typedi';

@Service()
export class NotificationSubscriberService implements OnModuleInit {
  constructor(
    private readonly kafkaService: KafkaService,
    private readonly notificationService: NotificationService,
  ) {}

  async onModuleInit(): Promise<void> {
    await this.kafkaService.connect();
    this.kafkaService.on('notifications', this.handleNotificationReceiver.bind(this));
  }

  private async handleNotificationReceiver(payload: EachMessagePayload): Promise<void> {
    const { message } = payload;
    const value = message.value?.toString();

    try {
      const notificationMessage: KafkaMessage = value ? JSON.parse(value) : null;

      switch (notificationMessage.eventType) {
        case NotificationAction.USER_CREATED:
          await this.handleUserCreated(notificationMessage);
          break;
        default:
          console.error('❌ Unknown event type:', notificationMessage.eventType);
          break;
      }
    } catch (err) {
      console.error('❌ Failed to process notification message:', err);
    }
  }

  private async handleUserCreated(kafkaMessage: KafkaMessage): Promise<void> {
    const { isValid, message, data } = this.validateRequiredFields<UserCreatedData>(kafkaMessage);
    if (!isValid || !data) {
      console.error('❌ Missing field:', message);
      return;
    }

    await this.notificationService.createNotification(data.userId, kafkaMessage.id, 'A new user has been created!');
  }

  private validateRequiredFields<T>(message: KafkaMessage): {
    isValid: boolean;
    message: string;
    data?: T;
    missingFields?: string[];
  } {
    if (!message.data) {
      return {
        isValid: false,
        message: `Missing data for ${message.id} event`,
      };
    }

    const requiredFields = Object.keys(message.data) as (keyof T)[];
    const missingFields = requiredFields.filter((field) => {
      const value = (message.data as T)[field];
      return value === undefined || value === null;
    });

    if (missingFields.length > 0) {
      return {
        isValid: false,
        message: `Missing required fields for ${message.id} event: ${missingFields.join(', ')}`,
        missingFields: missingFields as string[],
      };
    }

    return {
      isValid: true,
      message: 'valid',
      data: message.data as T,
    };
  }
}
