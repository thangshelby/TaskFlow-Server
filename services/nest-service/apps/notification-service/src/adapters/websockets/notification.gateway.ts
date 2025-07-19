import { WebSocketGateway, OnGatewayInit, OnGatewayConnection, OnGatewayDisconnect, MessageBody, SubscribeMessage, ConnectedSocket } from '@nestjs/websockets';
import { Server, Socket } from 'socket.io';

@WebSocketGateway({
  cors: {
    origin: '*',
  },
})
export class NotificationsGateway implements OnGatewayInit, OnGatewayConnection, OnGatewayDisconnect {
  private server: Server;

  afterInit(server: Server) {
    this.server = server;
    console.log('Socket.IO initialized');
  }

  handleConnection(client: Socket) {
    console.log(`Client connected: ${client.id}`);
  }

  handleDisconnect(client: Socket) {
    console.log(`Client disconnected: ${client.id}`);
  }
  @SubscribeMessage('join')
  async handleJoin(@MessageBody() data: { userId: string }, @ConnectedSocket() client: Socket) {
    console.log(`User ${data.userId} joined`);
    await client.join(`user-${data.userId}`);
  }

  sendNotification(userId: string) {
    console.log(`Sending notification to user ${userId}`);
    this.server.to(`user-${userId}`).emit('new-notification');
  }
}
