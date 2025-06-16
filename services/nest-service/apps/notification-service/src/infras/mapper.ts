import { NotificationDomain } from '@notification-service/core/models/notification';
import { NotificationRes } from '@notification-service/types/notification_service/notification';

export class NotificationMapper {
  static toNotiResponse(domain: NotificationDomain): NotificationRes {
    return {
      recipientId: domain.recipientId,
      actorId: domain.actorId || '',
      type: domain.type,
      referenceId: domain.referenceId || '',
      referenceType: domain.referenceType || '',
      content: domain.content,
      isRead: domain.isRead,
      createdAt: domain.createdAt.toISOString(),
    };
  }
  static toNotiResponseList(domains: NotificationDomain[]): NotificationRes[] {
    return domains.map(this.toNotiResponse);
  }
}
