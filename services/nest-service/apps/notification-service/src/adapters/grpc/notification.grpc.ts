import { Metadata } from '@grpc/grpc-js';
import { UserClientService } from '@nest-service/core';
import { Controller, Logger } from '@nestjs/common';
import { GrpcMethod } from '@nestjs/microservices';
import { NotificationService } from '@notification-service/core/services/notification.service';
import { NotificationMapper } from '@notification-service/infras/mapper';
import { CreateNotificationReq, CreateNotificationRes } from '@notification-service/types/notification_service/notification';
@Controller()
export class NotificationGrpcController {
  constructor(
    private readonly notificationService: NotificationService,
    private readonly userClientService: UserClientService,
  ) {}

  @GrpcMethod('notification_service.NotificationService', 'SendNotification')
  async sendNotification(data: CreateNotificationReq, metadata: Metadata): Promise<CreateNotificationRes> {
    const user = await this.userClientService.getUserById({ metadata });

    Logger.log('UserRole:', user);
    const type = this.notificationService.ValidateNotificationType(data.type);

    const noti = await this.notificationService.createNotification({
      content: data.content || '',
      createdAt: new Date(),
      isRead: false,
      recipientId: data.recipientId,
      type: type,
      actorId: data.actorId,
      referenceId: data.recipientId,
      referenceType: data.referenceType,
    });

    return {
      status: 'success',
      message: 'Notification sent!',
      data: NotificationMapper.toNotiResponse(noti),
    };
  }
}
