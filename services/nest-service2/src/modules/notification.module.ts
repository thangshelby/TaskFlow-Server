import { Module } from '@nestjs/common';
import { NotificationRepo } from 'src/repos/notification.repo';
import { KafkaService } from 'src/services/kafka.service';
import { NotificationSubscriberService } from 'src/services/notification-subcriber.service';
import { NotificationService } from 'src/services/notification.service';

@Module({
  providers: [NotificationSubscriberService, KafkaService, NotificationService, NotificationRepo],
})
export class NotificationModule {}
