import { forwardRef, Inject, Injectable, Logger } from '@nestjs/common';
import { WebSocketGateway, WebSocketServer, OnGatewayConnection, OnGatewayDisconnect } from '@nestjs/websockets';
import { NotificationService } from '@notification-service/core/services/notification.service';
import { Server, Socket } from 'socket.io';
import convert from '@notification-service/utils/convert.utils';

@Injectable()
export class NotificationEmitterService {
  private server: Server;
  constructor(@Inject(forwardRef(() => NotificationService)) private readonly notificationService: NotificationService) {}

  setServer(server: Server) {
    this.server = server;
  }

  broadcast(event: string, data: any) {
    if (this.server) {
      this.server.emit(event, data);
    }
  }

  async sendToUser(userId: string) {
    if (this.server) {
      // TODO: handle notification paging
      const notis = await this.notificationService.listNotifications({
        page: 1,
        limit: 100,
        userId: userId,
      });

      this.server.to(userId).emit(
        'refresh-list',
        convert.convertToSnakeCase({
          notifications: notis.data,
          total: notis.totalCount,
        }),
      );
    }
  }
}

@Injectable()
@WebSocketGateway({
  cors: {
    origin: '*',
  },
})
export class NotificationGateway implements OnGatewayConnection, OnGatewayDisconnect {
  @WebSocketServer()
  server: Server;
  constructor(private readonly emitter: NotificationEmitterService) {}

  afterInit(server: Server) {
    this.emitter.setServer(server);
  }

  async handleConnection(socket: Socket) {
    const userId = socket.handshake.query.userId;
    if (userId && typeof userId === 'string') {
      await socket.join(userId);

      Logger.log(`User ${userId} connected`);

      await this.emitter.sendToUser(userId);
    } else {
      socket.disconnect();
      Logger.warn(`Socket disconnected due to missing userId`);
    }
  }

  handleDisconnect(socket: Socket) {
    Logger.log(`Client disconnected: ${socket.id}`);
  }
}
