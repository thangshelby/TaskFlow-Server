import { NotificationDomain } from '@notification-service/core/models/notification';
import { INotification } from '@notification-service/infras/schema/notification.schema';

export class NotificationMapper {
  static toDomain(entity: INotification): NotificationDomain {
    return {
      id: entity._id.toString(),
      recipientId: entity.recipientId?.toString(),
      actorId: entity.actorId?.toString(),
      type: entity.type as any,
      referenceId: entity.referenceId,
      referenceType: entity.referenceType as any,
      content: entity.content,
      isRead: entity.isRead,
      createdAt: entity.createdAt.toISOString(),
    };
  }

  static toDomainList(entities: INotification[]): NotificationDomain[] {
    return entities.map((entity) => this.toDomain(entity));
  }
}
