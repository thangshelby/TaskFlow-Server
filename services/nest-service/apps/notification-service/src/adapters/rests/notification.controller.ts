import { Controller, Get } from '@nestjs/common';
import { LogService } from '@nest-service/core';
import { NotificationService } from '@notification-service/core/services/notification.service';

@Controller()
export class NotificationController {
  constructor(private readonly notificationService: NotificationService, private readonly logService: LogService) {}

  @Get()
  getData() {
    this.logService.log('Fetching notifications');
  }
}
