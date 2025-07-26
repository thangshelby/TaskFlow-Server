import { Controller, Post, Body } from '@nestjs/common';
import { NotificationService } from '@notification-service/core/services/notification.service';

interface CreateNotificationReq {
  recipientId: string;
  actorId?: string | undefined;
  type: string;
  referenceId?: string | undefined;
  referenceType?: string | undefined;
  content?: string | undefined;
  isRead: boolean;
}

interface GetAllNotificationsReq {
  userId?: string | undefined;
  projectId?: string | undefined;
  sprintId?: string | undefined;
  page?: number | undefined;
  limit?: number | undefined;
}

@Controller('/api/v1/notifications')
export class NotificationController {
  constructor(private readonly notificationService: NotificationService) {}

  @Post()
  async sendNotification(@Body() data: CreateNotificationReq) {
    const type = this.notificationService.ValidateNotificationType(data.type);
    const noti = await this.notificationService.createNotification({
      content: data.content || '',
      createdAt: new Date(),
      isRead: false,
      recipientId: data.recipientId,
      type: type,
      actorId: data.actorId,
      referenceId: data.recipientId,
    });

    // TODO: emit through WebSocket here later

    return {
      status: 'success',
      message: 'Notification sent!',
      data: noti,
    };
  }

  @Post('/get-all')
  async getAllNotifications(@Body() body: GetAllNotificationsReq) {
    const { data: notis, totalCount } = await this.notificationService.listNotifications(body);

    const currentPage = body.page || 1;
    const limit = body.limit || 10;
    const totalPages = Math.ceil(totalCount / limit);

    return {
      status: 'success',
      message: 'Get all notification success!',
      pagination: {
        currentPage,
        limit,
        totalItems: totalCount,
        totalPages,
      },
      data: notis,
    };
  }
}
