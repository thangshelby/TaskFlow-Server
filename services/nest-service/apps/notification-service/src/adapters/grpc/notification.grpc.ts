import { Controller } from '@nestjs/common';
import { GrpcMethod } from '@nestjs/microservices';
import { NotificationResponse } from '@notification-service/types/notification';

@Controller()
export class NotificationGrpcController {
  @GrpcMethod('notification_service.NotificationService', 'SendNotification')
  async sendNotification(data: { title: string; message: string; userId: string }): Promise<NotificationResponse> {
    return {
      success: true,
      message: 'Notification sent!',
    };
  }
}
