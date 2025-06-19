import { Injectable } from '@nestjs/common';
import { NotificationDomain, NotificationType } from '@notification-service/core/models/notification';
import { INotificationRepo } from '@notification-service/core/interfaces/notification-repo.interface';
import { RpcException } from '@nestjs/microservices';
import { status } from '@grpc/grpc-js';
export interface CreateNotificationParams {
  recipientId: string;
  actorId?: string;
  type: NotificationType;
  referenceId?: string;
  content: string;
  isRead: boolean;
  createdAt: Date;
}

@Injectable()
export class NotificationService {
  constructor(private readonly notificationRepo: INotificationRepo) {}

  async createNotification(data: CreateNotificationParams): Promise<NotificationDomain> {
    const refType = this.getReferenceTypeByNotification(data.type);

    return await this.notificationRepo.create({
      recipientId: data.recipientId,
      actorId: data.actorId,
      type: data.type,
      referenceId: data.referenceId,
      referenceType: refType,
      content: data?.content || '',
      isRead: false,
      createdAt: new Date(),
    });
  }

  private getReferenceTypeByNotification(type: NotificationType): string {
    switch (type) {
      case NotificationType.ASSIGNMENT:
      case NotificationType.MENTION:
      case NotificationType.COMMENT:
      case NotificationType.STATUS_UPDATE:
      case NotificationType.DUE_DATE_REMINDER:
        return 'issue';

      case NotificationType.SPRINT_STARTED:
        return 'sprint';

      case NotificationType.PROJECT_INVITATION:
      case NotificationType.PROJECT_ADDED:
        return 'project';

      case NotificationType.REACTION:
        return 'comment';

      case NotificationType.SYSTEM_ALERT:
        return 'system';

      case NotificationType.PROJECT_TEAM_ADDED:
        return 'project_member';

      default:
        return '';
    }
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
