// src/notification/infras/repos/notification.repo.ts
import { Injectable } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { Model } from 'mongoose';
import { NotificationDomain } from '@notification-service/core/models/notification';
import { INotificationRepo } from '@notification-service/core/interfaces/notification-repo.interface';
import { INotification, NotificationDocument } from '@notification-service/infras/schema/notification.schema';

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
}
