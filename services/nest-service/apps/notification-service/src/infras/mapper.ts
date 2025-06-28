import { NotificationRes } from '@nest-service/core/types/base';
import { NotificationDomain } from '@notification-service/core/models/notification';
import { INotification } from '@notification-service/infras/schema/notification.schema';

export class NotificationMapper {
  static toNotiResponse(domain: NotificationDomain): NotificationRes {
    return {
      recipientId: domain.recipientId,
      actorId: domain.actorId || '',
      type: domain.type,
      referenceId: domain.referenceId || '',
      referenceType: domain.referenceType || '',
      content: domain.content || '',
      isRead: domain.isRead,
      createdAt: domain.createdAt.toISOString(),
      referenceData: JSON.stringify(domain?.referenceData),
    };
  }
  static toNotiResponseList(domains: NotificationDomain[]): NotificationRes[] {
    return domains.map((domain) => this.toNotiResponse(domain));
  }
  static toDomain(entity: INotification): NotificationDomain {
    return {
      recipientId: entity.recipientId?.toString(),
      actorId: entity.actorId?.toString(),
      type: entity.type as any,
      referenceId: entity.referenceId,
      referenceType: entity.referenceType as any,
      content: entity.content,
      isRead: entity.isRead,
      createdAt: new Date(entity.createdAt),
    };
  }

  static toDomainList(entities: INotification[]): NotificationDomain[] {
    return entities.map((entity) => this.toDomain(entity));
  }
}
