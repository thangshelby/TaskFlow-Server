import { Service } from 'typedi';
import { INotification } from 'src/common/models/notification';
import { CassandraBaseRepo } from 'src/repos/cassandra.repo';

@Service()
export class NotificationRepo extends CassandraBaseRepo {
  constructor() {
    super('taskflow');
  }

  async createNotification(notification: INotification): Promise<void> {
    return;
    const query = 'INSERT INTO notifications (id, user_id, event_type, message, created_at) VALUES (?, ?, ?, ?, ?)';
    const params = [
      notification.id,
      notification.userId,
      notification.eventType,
      notification.message,
      notification.createdAt,
    ];

    try {
      await this.executeQuery(query, params, { prepare: true });
      console.log('Notification created successfully!');
    } catch (error) {
      console.error('Error creating notification:', error);
      throw error;
    }
  }
}
