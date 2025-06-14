import { Module } from '@nestjs/common';
import { CoreModule } from '@nest-service/core';
import { ConfigModule } from '@nestjs/config';
import { NotificationService } from '@notification-service/core/services/notification.service';
import { NotificationSubscriberService } from '@notification-service/core/services/notification-subcriber.service';
import { NotificationRepo } from '@notification-service/infras/notification.repo';
import { NotificationGrpcController } from '@notification-service/adapters/grpc/notification.grpc';
@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
      envFilePath: ['.env'],
    }),
    CoreModule,
  ],
  controllers: [NotificationGrpcController],
  providers: [NotificationService, NotificationSubscriberService, NotificationRepo],
})
export class NotificationModule {}
