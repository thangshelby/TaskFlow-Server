import { v4 as uuidv4 } from 'uuid';
import { INotification } from 'src/models/notification';
import { NotificationRepo } from 'src/repos/notification.repo';
import { Service } from 'typedi';

@Service()
export class NotificationService {
  constructor(private readonly notificationRepo: NotificationRepo) {}

  async createNotification(userId: string, eventType: string, message: string): Promise<void> {
    const notification: INotification = {
      id: uuidv4(),
      userId,
      eventType,
      message,
      createdAt: new Date(),
    };

    try {
      // Save the notification to Cassandra
      await this.notificationRepo.createNotification(notification);
      console.log('Notification created for user:', userId);
    } catch (error) {
      console.error('Error creating notification:', error);
      throw error;
    }
  }
}
