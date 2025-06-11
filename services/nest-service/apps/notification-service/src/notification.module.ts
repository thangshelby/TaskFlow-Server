import { Module } from '@nestjs/common';
import { CoreModule } from '@nest-service/core';
import { ConfigModule } from '@nestjs/config';
import { NotificationService } from '@notification-service/core/services/notification.service';
import { NotificationController } from '@notification-service/adapters/rests/notification.controller';
import { NotificationSubscriberService } from '@notification-service/core/services/notification-subcriber.service';
import { NotificationRepo } from '@notification-service/infras/notification.repo';
@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
      envFilePath: ['.env'],
    }),
    CoreModule,
  ],
  controllers: [NotificationController],
  providers: [NotificationService, NotificationSubscriberService, NotificationRepo],
})
export class NotificationModule {}
