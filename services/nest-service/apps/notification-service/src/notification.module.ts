import { Module } from '@nestjs/common';
import { CoreModule } from '@nest-service/core';
import { ConfigModule } from '@nestjs/config';
import { NotificationService } from '@notification-service/core/services/notification.service';
import { NotificationSubscriberService } from '@notification-service/core/services/subcribers/notification-subcriber.service';
import { MailSubscriberService } from '@notification-service/core/services/subcribers/mail-subcriber.service';
import { NotificationRepo } from '@notification-service/infras/repos/notification.repo';
import { MongooseModule } from '@nestjs/mongoose';
import { INotificationRepo } from '@notification-service/core/interfaces/notification-repo.interface';
import { INotification, NotificationSchema } from '@notification-service/infras/schema/notification.schema';
import { NotificationController } from '@notification-service/adapters/controllers/notification.controller';
import { HealthController } from '@notification-service/adapters/controllers/health.controller';
import { NotificationEmitterService, NotificationGateway } from '@notification-service/adapters/websocket/notification.websocket';
import { MailService } from '@notification-service/core/services/mail.service';
import { IMailSender } from '@notification-service/core/interfaces/mail-sender.interface';
import { MailSenderRepo } from '@notification-service/infras/repos/mail-sender.repo';

@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
      envFilePath: ['.env'],
    }),
    CoreModule,
    MongooseModule.forFeature([{ name: INotification.name, schema: NotificationSchema }]),
  ],
  controllers: [NotificationController, HealthController],
  providers: [
    NotificationGateway,
    MailService,
    NotificationService,
    // MailSubscriberService,
    NotificationSubscriberService,
    NotificationEmitterService,
    {
      provide: INotificationRepo,
      useClass: NotificationRepo,
    },
    {
      provide: IMailSender,
      useClass: MailSenderRepo,
    },
  ],
})
export class NotificationModule {}
