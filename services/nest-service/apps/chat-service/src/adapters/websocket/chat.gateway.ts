import {
  WebSocketGateway,
  WebSocketServer,
  SubscribeMessage,
  OnGatewayConnection,
  OnGatewayDisconnect,
  ConnectedSocket,
  MessageBody,
} from '@nestjs/websockets';
import { Server, Socket } from 'socket.io';
import { ChatService } from '../../core/services/chat.service';
import { UserClientService } from '@nest-service/core';
import { MessageType } from '../../core/models/chat';
import { ChatMapper } from '../../infras/mapper';
import { Logger } from '@nestjs/common';

@WebSocketGateway({
  cors: {
    origin: '*',
  },
  namespace: 'chat',
})
export class ChatGateway implements OnGatewayConnection, OnGatewayDisconnect {
  @WebSocketServer() server: Server;
  private logger = new Logger('ChatGateway');
  private userSockets: Map<string, Set<string>> = new Map(); // userId -> Set of socketIds
  private typingUsers: Map<string, Set<string>> = new Map(); // roomId -> Set of userIds

  constructor(
    private readonly chatService: ChatService,
    private readonly userClientService: UserClientService,
  ) {}

  async handleConnection(client: Socket) {
    try {
      const user = await this.userClientService.getUserById({ metadata: client.handshake.auth?.token });
      if (!user) {
        client.disconnect();
        return;
      }

      // Store socket connection
      if (!this.userSockets.has(user.id)) {
        this.userSockets.set(user.id, new Set());
      }
      this.userSockets.get(user.id).add(client.id);

      // Join user's rooms
      const rooms = await this.chatService.getRoomsByUserId(user.id);
      rooms.forEach(room => {
        client.join(room.id);
      });

      this.logger.log(`Client connected: ${client.id} - User: ${user.id}`);
    } catch (error) {
      this.logger.error(`Connection error: ${error.message}`);
      client.disconnect();
    }
  }

  handleDisconnect(client: Socket) {
    // Remove socket from user's connections
    this.userSockets.forEach((sockets, userId) => {
      if (sockets.has(client.id)) {
        sockets.delete(client.id);
        if (sockets.size === 0) {
          this.userSockets.delete(userId);
        }
      }
    });

    // Remove from typing indicators
    this.typingUsers.forEach((users, roomId) => {
      users.forEach(userId => {
        if (!this.userSockets.has(userId)) {
          users.delete(userId);
          this.server.to(roomId).emit('userStoppedTyping', { roomId, userId });
        }
      });
    });

    this.logger.log(`Client disconnected: ${client.id}`);
  }

  @SubscribeMessage('joinRoom')
  async handleJoinRoom(@ConnectedSocket() client: Socket, @MessageBody() roomId: string) {
    try {
      const user = await this.userClientService.getUserById({ metadata: client.handshake.auth?.token });
      const room = await this.chatService.getRoomById(roomId);

      if (!room.members.includes(user.id)) {
        client.emit('error', { message: 'Not a member of this room' });
        return;
      }

      client.join(roomId);
      this.server.to(roomId).emit('userJoined', { roomId, userId: user.id });
    } catch (error) {
      client.emit('error', { message: error.message });
    }
  }

  @SubscribeMessage('leaveRoom')
  async handleLeaveRoom(@ConnectedSocket() client: Socket, @MessageBody() roomId: string) {
    try {
      const user = await this.userClientService.getUserById({ metadata: client.handshake.auth?.token });
      client.leave(roomId);
      this.server.to(roomId).emit('userLeft', { roomId, userId: user.id });
    } catch (error) {
      client.emit('error', { message: error.message });
    }
  }

  @SubscribeMessage('sendMessage')
  async handleMessage(
    @ConnectedSocket() client: Socket,
    @MessageBody() data: { roomId: string; content: string; type: string; replyToId?: string },
  ) {
    try {
      const user = await this.userClientService.getUserById({ metadata: client.handshake.auth?.token });
      const type = this.chatService.validateMessageType(data.type);

      const message = await this.chatService.createMessage({
        roomId: data.roomId,
        senderId: user.id,
        content: data.content,
        type,
        replyToId: data.replyToId,
      });

      const messageResponse = ChatMapper.toMessageResponse(message);
      this.server.to(data.roomId).emit('messageReceived', messageResponse);

      // Clear typing indicator for this user
      this.handleStopTyping(client, data.roomId);
    } catch (error) {
      client.emit('error', { message: error.message });
    }
  }

  @SubscribeMessage('startTyping')
  async handleStartTyping(@ConnectedSocket() client: Socket, @MessageBody() roomId: string) {
    try {
      const user = await this.userClientService.getUserById({ metadata: client.handshake.auth?.token });
      
      if (!this.typingUsers.has(roomId)) {
        this.typingUsers.set(roomId, new Set());
      }
      
      const typingSet = this.typingUsers.get(roomId);
      if (!typingSet.has(user.id)) {
        typingSet.add(user.id);
        this.server.to(roomId).emit('userStartedTyping', { roomId, userId: user.id });
      }
    } catch (error) {
      client.emit('error', { message: error.message });
    }
  }

  @SubscribeMessage('stopTyping')
  async handleStopTyping(@ConnectedSocket() client: Socket, @MessageBody() roomId: string) {
    try {
      const user = await this.userClientService.getUserById({ metadata: client.handshake.auth?.token });
      
      if (this.typingUsers.has(roomId)) {
        const typingSet = this.typingUsers.get(roomId);
        if (typingSet.has(user.id)) {
          typingSet.delete(user.id);
          this.server.to(roomId).emit('userStoppedTyping', { roomId, userId: user.id });
        }
      }
    } catch (error) {
      client.emit('error', { message: error.message });
    }
  }
}