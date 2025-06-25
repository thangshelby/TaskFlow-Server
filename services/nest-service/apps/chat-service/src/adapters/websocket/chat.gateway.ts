import { WebSocketGateway, WebSocketServer, SubscribeMessage, OnGatewayConnection, OnGatewayDisconnect, ConnectedSocket, MessageBody } from '@nestjs/websockets';
import { Server, Socket } from 'socket.io';
import { ChatService } from '../../core/services/chat.service';
import { UserClientService } from '@nest-service/core';
import { RoomDomain } from '../../core/models/chat';
import { ChatMapper } from '../../infras/mapper';
import { Logger, Injectable, UnauthorizedException } from '@nestjs/common';

interface AuthSocket extends Socket {
  handshake: Socket['handshake'] & {
    auth: { token?: string };
  };
}

interface SocketNamespace {
  sockets: Map<string, AuthSocket>;
}

@Injectable()
@WebSocketGateway(0, {
  namespace: 'chat',
  cors: { origin: '*' },
})
export class ChatGateway implements OnGatewayConnection, OnGatewayDisconnect {
  @WebSocketServer() private readonly server!: Server & { sockets: SocketNamespace };
  private readonly logger = new Logger(ChatGateway.name);
  // Store socket connections and states
  private userSockets = new Map<string, Set<string>>(); // userId -> Set of socketIds
  private typingUsers = new Map<string, Set<string>>(); // roomId -> Set of userIds
  private userRooms = new Map<string, Set<string>>(); // userId -> Set of roomIds

  constructor(
    private readonly chatService: ChatService,
    private readonly userClientService: UserClientService,
  ) {}

  private handleSocketError(client: Socket, message: string) {
    try {
      client.emit('error', { message });
      client.disconnect();
    } catch (error) {
      this.logger.error(`Error handling socket error: ${error}`);
    }
  }

  async handleConnection(client: Socket) {
    try {
      const token = client.handshake.auth?.token;
      if (!token) {
        this.handleSocketError(client, 'Authentication token required');
        return;
      }

      const user = await this.userClientService.getUserById({ metadata: token });
      if (!user?.id) {
        this.handleSocketError(client, 'User not authenticated');
        return;
      }

      // Store socket connection
      if (!this.userSockets.has(user.id)) {
        this.userSockets.set(user.id, new Set());
      }
      const userSocketSet = this.userSockets.get(user.id);
      if (userSocketSet) {
        userSocketSet.add(client.id);
      }

      // Join user's rooms
      const rooms = await this.chatService.getRoomsByUserId(user.id);
      rooms.forEach((room) => {
        if (room.id) {
          client.join(room.id);
        }
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
      users.forEach((userId) => {
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

      if (!user?.id || !room.members.includes(user.id)) {
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
      if (!user?.id) {
        throw new UnauthorizedException('User not authenticated');
      }

      client.leave(roomId);
      this.server.to(roomId).emit('userLeft', { roomId, userId: user.id });
    } catch (error) {
      client.emit('error', { message: error.message });
    }
  }

  @SubscribeMessage('sendMessage')
  async handleMessage(@ConnectedSocket() client: Socket, @MessageBody() data: { roomId: string; content: string; type: string; replyToId?: string }) {
    try {
      const user = await this.userClientService.getUserById({ metadata: client.handshake.auth?.token });
      const type = this.chatService.validateMessageType(data.type);

      if (!user?.id) {
        throw new UnauthorizedException('User not authenticated');
      }

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
      if (!user?.id) {
        throw new UnauthorizedException('User not authenticated');
      }

      if (typingSet && !typingSet.has(user.id)) {
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

      if (!user?.id) {
        throw new UnauthorizedException('User not authenticated');
      }

      const typingSet = this.typingUsers.get(roomId);
      if (typingSet?.has(user.id)) {
        typingSet.delete(user.id);
        this.server.to(roomId).emit('userStoppedTyping', { roomId, userId: user.id });
      }
    } catch (error) {
      client.emit('error', { message: error.message });
    }
  }

  private getSocketsForUser(userId: string): AuthSocket[] {
    const socketIds = this.userSockets.get(userId);
    if (!socketIds?.size) return [];

    const sockets: AuthSocket[] = [];
    socketIds.forEach((id) => {
      const socket = this.server.sockets.sockets.get(id);
      if (socket) {
        sockets.push(socket);
      }
    });
    return sockets;
  }

  notifyUserOfNewRoom(userId: string, room: RoomDomain) {
    if (!room?.id) {
      this.logger.warn('Attempted to notify about room with no ID');
      return;
    }

    try {
      const roomResponse = ChatMapper.toRoomResponse(room);
      const sockets = this.getSocketsForUser(userId);

      sockets.forEach((socket) => {
        socket.join(room.id!);
        socket.emit('roomCreated', roomResponse);
      });

      // Track room membership
      const userRooms = this.userRooms.get(userId) || new Set<string>();
      userRooms.add(room.id);
      this.userRooms.set(userId, userRooms);
    } catch (error) {
      this.logger.error(`Failed to notify user ${userId} about room ${room.id}: ${error}`);
    }
  }

  addUserToRoom(userId: string, roomId: string) {
    try {
      const sockets = this.getSocketsForUser(userId);
      for (const socket of sockets) {
        socket.join(roomId);
      }

      // Track room membership
      const userRooms = this.userRooms.get(userId) || new Set<string>();
      userRooms.add(roomId);
      this.userRooms.set(userId, userRooms);
    } catch (error) {
      this.logger.error(`Failed to add user ${userId} to room ${roomId}: ${error}`);
    }
  }

  removeUserFromRoom(userId: string, roomId: string) {
    try {
      const sockets = this.getSocketsForUser(userId);
      for (const socket of sockets) {
        socket.leave(roomId);
      }

      // Update room tracking
      const userRooms = this.userRooms.get(userId);
      if (userRooms) {
        userRooms.delete(roomId);
        if (userRooms.size === 0) {
          this.userRooms.delete(userId);
        }
      }
    } catch (error) {
      this.logger.error(`Failed to remove user ${userId} from room ${roomId}: ${error}`);
    }
  }
}
