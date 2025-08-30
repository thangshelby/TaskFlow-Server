import { KafkaActionType, KafkaMessage, KafkaService } from '@nest-service/core';
import { Injectable, OnModuleInit } from '@nestjs/common';
import { NotificationMessageData } from '@notification-service/core/models/notification';
import { CreateNotificationParams, NotificationService } from '@notification-service/core/services/notification.service';
import validator from '@notification-service/utils/validate.utils';
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
          console.error('❌ [NOTIFICATION_TOPIC] Unknown event type:', notificationMessage.eventType);
          break;
      }
    } catch (err) {
      console.error('❌ [NOTIFICATION_TOPIC] Failed to process notification message:', err);
    }
  }

  private async handleCreateNotification(kafkaMessage: KafkaMessage): Promise<void> {
    const { isValid, message, data } = validator.validateRequiredFields<NotificationMessageData>(kafkaMessage);
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
}
