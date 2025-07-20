import { Module } from '@nestjs/common';
import { CoreModule } from '@nest-service/core';
import { ConfigModule } from '@nestjs/config';
import { NotificationService } from '@notification-service/core/services/notification.service';
import { NotificationSubscriberService } from '@notification-service/core/services/notification-subcriber.service';
import { NotificationRepo } from '@notification-service/infras/repos/notification.repo';
import { NotificationGrpcController } from '@notification-service/adapters/grpc/notification.grpc';
import { MongooseModule } from '@nestjs/mongoose';
import { INotificationRepo } from '@notification-service/core/interfaces/notification-repo.interface';
import { INotification, NotificationSchema } from '@notification-service/infras/schema/notification.schema';

@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
      envFilePath: ['.env'],
    }),
    CoreModule,
    MongooseModule.forFeature([{ name: INotification.name, schema: NotificationSchema }]),
  ],
  controllers: [NotificationGrpcController],
  providers: [
    NotificationService,
    NotificationSubscriberService,
    {
      provide: INotificationRepo,
      useClass: NotificationRepo,
    },
  ],
})
export class NotificationModule {}
