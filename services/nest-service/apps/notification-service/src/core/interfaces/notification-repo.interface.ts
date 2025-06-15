// src/notification/domain/interfaces/notification-repo.interface.ts

import { NotificationDomain } from '@notification-service/core/models/notification';

export abstract class INotificationRepo {
  abstract create(notification: NotificationDomain): Promise<NotificationDomain>;
}
