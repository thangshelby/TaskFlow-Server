import { Injectable } from '@nestjs/common';
import { NotificationDomain } from '@notification-service/core/models/notification';
import { INotificationRepo } from '@notification-service/core/interfaces/notification-repo.interface';
@Injectable()
export class NotificationService {
  constructor(private readonly notificationRepo: INotificationRepo) {}

  async createNotification(data: NotificationDomain): Promise<NotificationDomain> {
    return await this.notificationRepo.create(data);
  }
}
