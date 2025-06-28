import { NotificationDomain } from '@notification-service/core/models/notification';
import { GetAllNotificationParams } from '@notification-service/core/services/notification.service';

export abstract class INotificationRepo {
  abstract create(notification: NotificationDomain): Promise<NotificationDomain>;
  abstract listAll(params: GetAllNotificationParams): Promise<NotificationDomain[]>;
  abstract countAll(params: GetAllNotificationParams): Promise<number>;
}
