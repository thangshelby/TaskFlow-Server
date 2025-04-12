import { Service } from 'typedi';
import { Client } from 'cassandra-driver';
import { INotification } from 'src/models/notification';

@Service()
export class NotificationRepo {
  private client: Client;

  constructor() {
    this.client = new Client({
      contactPoints: ['localhost'],
      localDataCenter: 'datacenter1',
      keyspace: 'notifications',
    });
  }

  // Method to create a notification
  async createNotification(notification: INotification): Promise<void> {
    const query = 'INSERT INTO notifications (id, user_id, event_type, message, created_at) VALUES (?, ?, ?, ?, ?)';
    const params = [
      notification.id,
      notification.userId,
      notification.eventType,
      notification.message,
      notification.createdAt,
    ];

    try {
      await this.client.execute(query, params, { prepare: true });
      console.log('Notification created successfully!');
    } catch (error) {
      console.error('Error creating notification:', error);
      throw error;
    }
  }
}
