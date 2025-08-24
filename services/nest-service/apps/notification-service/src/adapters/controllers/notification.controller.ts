import { Controller, Post, Body, Put, Param } from '@nestjs/common';
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

interface UpdateNotificationsReq {
  isRead: boolean;
}

interface BulkUpdateNotificationsReq {
  isRead: boolean;
  userId: string;
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

  @Put(':notiId')
  async updateNotification(@Param('notiId') notiId: string, @Body() body: UpdateNotificationsReq) {
    const noti = await this.notificationService.updateNotification({
      notiId: notiId,
      isRead: body.isRead,
    });

    return {
      status: 'success',
      message: 'Update notification success!',
      data: noti,
    };
  }

  @Post('/update-all')
  async bulkUpdateNotification(@Body() body: BulkUpdateNotificationsReq) {
    const res = await this.notificationService.bulkUpdateNotification({
      isRead: body.isRead,
      userId: body.userId,
    });

    return {
      status: 'success',
      message: 'Update notification success!',
      data: res,
    };
  }
}
