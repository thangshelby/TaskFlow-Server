// src/notification/notification-grpc.controller.ts
import { Controller, Logger } from '@nestjs/common';
import { GrpcMethod } from '@nestjs/microservices';
import { NotificationService } from '@notification-service/core/services/notification.service';
import { NotificationMapper } from '@notification-service/infras/mapper';
import { CreateNotificationReq, CreateNotificationRes } from '@notification-service/types/notification';

@Controller()
export class NotificationGrpcController {
  constructor(private readonly notificationService: NotificationService) {}

  @GrpcMethod('notification_service.NotificationService', 'SendNotification')
  async sendNotification(data: CreateNotificationReq): Promise<CreateNotificationRes> {
    const noti = await this.notificationService.createNotification({
      content: data.content,
      createdAt: new Date(),
      isRead: false,
      recipientId: data.recipientId,
      type: data.type,
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
