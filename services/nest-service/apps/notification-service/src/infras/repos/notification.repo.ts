// src/notification/infras/repos/notification.repo.ts
import { Injectable } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { Model } from 'mongoose';
import { NotificationDomain } from '@notification-service/core/models/notification';
import { INotificationRepo } from '@notification-service/core/interfaces/notification-repo.interface';
import { INotification, NotificationDocument } from '@notification-service/infras/schema/notification.schema';
import { GetAllNotificationParams } from '@notification-service/core/services/notification.service';
import { NotificationMapper } from '@notification-service/infras/mapper';

@Injectable()
export class NotificationRepo implements INotificationRepo {
  constructor(
    @InjectModel(INotification.name)
    private readonly notificationModel: Model<NotificationDocument>,
  ) {}

  async create(notification: NotificationDomain): Promise<NotificationDomain> {
    const newNotification = new this.notificationModel(notification);
    await newNotification.save();
    return notification;
  }

  async countAll(params?: GetAllNotificationParams): Promise<number> {
    const filter: any = {};

    if (params?.userId) {
      filter.recipientId = params.userId;
    }

    if (params?.projectId) {
      filter.referenceType = 'project';
      filter.referenceId = params.projectId;
    }

    if (params?.sprintId) {
      filter.referenceType = 'sprint';
      filter.referenceId = params.sprintId;
    }

    return this.notificationModel.countDocuments(filter);
  }

  async listAll(params: GetAllNotificationParams): Promise<NotificationDomain[]> {
    const filter: any = {};

    if (params.userId) {
      filter.recipientId = params.userId;
    }

    if (params.projectId) {
      filter.referenceType = 'project';
      filter.referenceId = params.projectId;
    }

    if (params.sprintId) {
      filter.referenceType = 'sprint';
      filter.referenceId = params.sprintId;
    }

    const page = params.page ?? 1;
    const limit = params.limit ?? 10;
    const skip = (page - 1) * limit;

    const notifications = await this.notificationModel.find(filter).sort({ createdAt: -1 }).skip(skip).limit(limit).lean<INotification[]>();
    return NotificationMapper.toDomainList(notifications);
  }
}
