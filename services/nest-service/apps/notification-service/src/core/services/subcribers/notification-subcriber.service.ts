import { IQueueService, QUEUE_SERVICE_TOKEN } from '@nest-service/core';
import { Inject, Injectable, OnModuleInit } from '@nestjs/common';
import { NotificationMessageData } from '@notification-service/core/models/notification';
import { CreateNotificationParams, NotificationService } from '@notification-service/core/services/notification.service';
import validator from '@notification-service/utils/validate.utils';
import { ConfigService } from '@nestjs/config';

@Injectable()
export class NotificationSubscriberService implements OnModuleInit {
  constructor(
    @Inject(QUEUE_SERVICE_TOKEN) private readonly queueService: IQueueService,
    private readonly notificationService: NotificationService,
    private readonly configService: ConfigService,
  ) {}

  async onModuleInit(): Promise<void> {
    const queueName =
      this.configService.get<string>(
        'NOTIFICATION_QUEUE_NAME',
        'https://sqs.ap-southeast-1.amazonaws.com/017263836577/notifications.fifo',
      ) || '';

    await this.queueService.subscribe(queueName, this.handleNotificationReceiver.bind(this));
  }

  private async handleNotificationReceiver(queueMessage: string): Promise<void> {
    try {
      await this.handleCreateNotification(queueMessage);
    } catch (err) {
      console.error('❌ [NOTIFICATION_TOPIC] Failed to process notification message:', err);
    }
  }

  private async handleCreateNotification(queueMessage: string): Promise<void> {
    const kafkaMessage = JSON.parse(queueMessage);
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
