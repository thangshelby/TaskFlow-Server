import { Injectable, Logger } from '@nestjs/common';
import { WebSocketGateway, WebSocketServer, OnGatewayConnection, OnGatewayDisconnect } from '@nestjs/websockets';
import { Server, Socket } from 'socket.io';

@Injectable()
@WebSocketGateway({
  cors: {
    origin: '*',
  },
})
export class NotificationGateway implements OnGatewayConnection, OnGatewayDisconnect {
  @WebSocketServer()
  server: Server;

  handleConnection(socket: Socket) {
    Logger.log(`User connected`);
    const userId = socket.handshake.query.userId;
    if (userId && typeof userId === 'string') {
      socket.join(userId);
      Logger.log(`User ${userId} connected`);
    } else {
      socket.disconnect();
      Logger.warn(`Socket disconnected due to missing userId`);
    }
  }

  handleDisconnect(socket: Socket) {
    Logger.log(`Client disconnected: ${socket.id}`);
  }
}
