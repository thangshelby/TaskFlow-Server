import { KafkaMessage, KafkaService } from '@nest-service/core';
import { Injectable, OnModuleInit } from '@nestjs/common';
import { UserCreatedData } from '@notification-service/core/models/notification';
import { NotificationService } from '@notification-service/core/services/notification.service';
import { EachMessagePayload } from 'kafkajs';

export const NOTIFICATION_KAFKA_TOPIC = 'notifications';

export const NotificationAction = {
  USER_CREATED: 'USER_CREATED_ACTION',
  ORDER_PLACED: 'ORDER_PLACED_ACTION',
  PAYMENT_RECEIVED: 'PAYMENT_RECEIVED_ACTION',
} as const;

export type NotificationActionType = (typeof NotificationAction)[keyof typeof NotificationAction];

@Injectable()
export class NotificationSubscriberService implements OnModuleInit {
  constructor(private readonly kafkaService: KafkaService, private readonly notificationService: NotificationService) {}

  async onModuleInit(): Promise<void> {
    this.kafkaService.on(NOTIFICATION_KAFKA_TOPIC, this.handleNotificationReceiver.bind(this));
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

    // await this.notificationService.createNotification(data.userId, kafkaMessage.id, 'A new user has been created!');
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
