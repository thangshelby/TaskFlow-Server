import { Injectable } from '@nestjs/common';
import { NotificationDomain, NotificationType } from '@notification-service/core/models/notification';
import { INotificationRepo } from '@notification-service/core/interfaces/notification-repo.interface';
import { RpcException } from '@nestjs/microservices';
import { status } from '@grpc/grpc-js';
@Injectable()
export class NotificationService {
  constructor(private readonly notificationRepo: INotificationRepo) {}

  async createNotification(data: CreateNotificationParams): Promise<NotificationDomain> {
    return await this.notificationRepo.create(data);
  }

  ValidateNotificationType(type: string): NotificationType {
    if (!type || !Object.values(NotificationType).includes(type as NotificationType)) {
      throw new RpcException({
        code: status.INVALID_ARGUMENT,
        message: 'Invalid notification type',
      });
    }
    return type as NotificationType;
  }
}
export interface CreateNotificationParams {
  recipientId: string;
  actorId?: string;
  type: NotificationType;
  referenceId?: string;
  referenceType?: string;
  content: string;
  isRead: boolean;
  createdAt: Date;
}
