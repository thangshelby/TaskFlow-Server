// import { Metadata } from '@grpc/grpc-js';
// import { UserClientService } from '@nest-service/core';
// import { CreateNotificationReq, CreateNotificationRes, GetAllNotificationsReq, GetAllNotificationsRes } from '@nest-service/core/types/notification_service/notification';
// import { Controller, Logger } from '@nestjs/common';
// import { GrpcMethod } from '@nestjs/microservices';
// import { NotificationService } from '@notification-service/core/services/notification.service';
// import { NotificationMapper } from '@notification-service/infras/mapper';
// @Controller()
// export class NotificationGrpcController {
//   constructor(
//     private readonly notificationService: NotificationService,
//     private readonly userClientService: UserClientService,
//   ) {}

//   @GrpcMethod('notification_service.NotificationService', 'SendNotification')
//   async sendNotification(data: CreateNotificationReq, metadata: Metadata): Promise<CreateNotificationRes> {
//     const user = await this.userClientService.getUserById({ metadata });

//     Logger.log('UserRole:', user);
//     const type = this.notificationService.ValidateNotificationType(data.type);

//     const noti = await this.notificationService.createNotification({
//       content: data.content || '',
//       createdAt: new Date(),
//       isRead: false,
//       recipientId: data.recipientId,
//       type: type,
//       actorId: data.actorId,
//       referenceId: data.recipientId,
//     });

//     return {
//       status: 'success',
//       message: 'Notification sent!',
//       data: NotificationMapper.toNotiResponse(noti),
//     };
//   }

//   @GrpcMethod('notification_service.NotificationService', 'GetAllNotifications')
//   async getAllNotifications(data: GetAllNotificationsReq): Promise<GetAllNotificationsRes> {
//     const { data: notis, totalCount } = await this.notificationService.listNotifications({
//       ...data,
//     });

//     const currentPage = data.page || 1;
//     const limit = data.limit || 10;
//     const totalPages = Math.ceil(totalCount / limit);

//     return {
//       status: 'success',
//       message: 'Get all notification success!',
//       pagination: {
//         currentPage,
//         limit,
//         totalItems: totalCount,
//         totalPages,
//       },
//       data: NotificationMapper.toNotiResponseList(notis),
//     };
//   }
// }
