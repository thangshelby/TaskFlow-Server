import { NotificationDomain } from '@notification-service/core/models/notification';
import { BulkUpdateNotificationParams, GetAllNotificationParams, UpdateNotificationParams } from '@notification-service/core/services/notification.service';

export abstract class INotificationRepo {
  abstract create(notification: NotificationDomain): Promise<NotificationDomain>;
  abstract listAll(params: GetAllNotificationParams): Promise<NotificationDomain[]>;
  abstract countAll(params: GetAllNotificationParams): Promise<number>;
  abstract update(params: UpdateNotificationParams): Promise<NotificationDomain>;
  abstract bulkUpdate(params: BulkUpdateNotificationParams): Promise<{ success: boolean }>;
}
