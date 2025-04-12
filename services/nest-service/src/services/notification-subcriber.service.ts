import { OnModuleInit } from '@nestjs/common';
import { EachMessagePayload } from 'kafkajs';
// import { UserCreatedData, validateRequiredFields } from 'src/models/notification';
import { NotificationAction } from 'src/models/notification.action';
import { KafkaMessage, KafkaService } from 'src/services/kafka.service';
import { NotificationService } from 'src/services/notification.service';
import { Service } from 'typedi';

@Service()
export class NotificationSubscriberService implements OnModuleInit {
  constructor(
    private readonly kafkaService: KafkaService,
    private readonly notificationService: NotificationService,
  ) {}

  async onModuleInit(): Promise<void> {
    await this.kafkaService.connect();
    this.kafkaService.on('notifications', this.handleNotification.bind(this));
  }

  private async handleNotification(payload: EachMessagePayload): Promise<void> {
    const { message } = payload;
    const value = message.value?.toString();

    try {
      const notificationMessage: KafkaMessage = value ? JSON.parse(value) : null;

      switch (notificationMessage.id) {
        case NotificationAction.USER_CREATED:
          // await this.handleUserCreated(notificationMessage);
          break;
        default:
          console.error('❌ Unknown event type:', notificationMessage.eventType);
          break;
      }
    } catch (err) {
      console.error('❌ Failed to process notification message:', err);
    }
  }

  // private async handleUserCreated(message: KafkaMessage): Promise<void> {
  //   const { isValid } = validateRequiredFields<UserCreatedData>(message);
  //   if (!isValid) {
  //     console.error('❌ Missing field');
  //     return;
  //   }

  //   const data = message.data as UserCreatedData;
  //   await this.notificationService.createNotification(data.userId, message.id, 'A new user has been created!');
  // }
}
