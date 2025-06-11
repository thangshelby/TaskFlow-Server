import { CassandraService } from '@nest-service/core';
import { Injectable } from '@nestjs/common';
import { INotification } from '@notification-service/core/models/notification';

@Injectable()
export class NotificationRepo {
  constructor(private readonly cassandraService: CassandraService) {}

  async createNotification(notification: INotification): Promise<void> {
    const query = 'INSERT INTO notifications (id, user_id, event_type, message, created_at) VALUES (?, ?, ?, ?, ?)';
    const params = [notification.id, notification.userId, notification.eventType, notification.message, notification.createdAt];

    try {
      await this.cassandraService.executeQuery(query, params, {
        prepare: true,
      });
      console.log('Notification created successfully!');
    } catch (error) {
      console.error('Error creating notification:', error);
      throw error;
    }
  }
}
