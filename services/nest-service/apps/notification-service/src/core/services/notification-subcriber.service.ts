import { KafkaActionType, KafkaMessage, KafkaService } from '@nest-service/core';
import { Injectable, Logger, OnModuleInit } from '@nestjs/common';
import { NotificationMessageData } from '@notification-service/core/models/notification';
import { CreateNotificationParams, NotificationService } from '@notification-service/core/services/notification.service';
import { EachMessagePayload } from 'kafkajs';

export const NOTIFICATION_KAFKA_TOPIC = 'notifications';

@Injectable()
export class NotificationSubscriberService implements OnModuleInit {
  constructor(
    private readonly kafkaService: KafkaService,
    private readonly notificationService: NotificationService,
  ) {}

  async onModuleInit(): Promise<void> {
    this.kafkaService.on(NOTIFICATION_KAFKA_TOPIC, this.handleNotificationReceiver.bind(this));
  }

  private async handleNotificationReceiver(payload: EachMessagePayload): Promise<void> {
    const { message } = payload;
    const value = message.value?.toString();

    try {
      const notificationMessage: KafkaMessage = value ? JSON.parse(value) : null;

      switch (notificationMessage.eventType) {
        case KafkaActionType.NOTIFICATIONS_CREATE_NEW_NOTIFICATION:
          await this.handleCreateNotification(notificationMessage);
          break;
        default:
          console.error('❌ Unknown event type:', notificationMessage.eventType);
          break;
      }
    } catch (err) {
      console.error('❌ Failed to process notification message:', err);
    }
  }

  private async handleCreateNotification(kafkaMessage: KafkaMessage): Promise<void> {
    Logger.log('receive message');
    const { isValid, message, data } = this.validateRequiredFields<NotificationMessageData>(kafkaMessage);
    if (!isValid || !data) {
      console.error('❌ Missing field:', message);
      return;
    }
    const type = this.notificationService.ValidateNotificationType(data.type);

    const notification: CreateNotificationParams = {
      recipientId: data.recipientId,
      type: type,
      content: ``,
      isRead: false,
      createdAt: new Date(),
      actorId: data.actorId,
      referenceId: data.issueId,
    };

    await this.notificationService.createNotification(notification);
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
