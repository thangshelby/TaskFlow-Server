/* eslint-disable */
import { WebSocketGateway, WebSocketServer } from '@nestjs/websockets';
import { Injectable, Logger } from '@nestjs/common';
import { ChatService } from '../../core/services/chat.service';
import { UserClientService } from '@nest-service/core';
import { WsAuthAdapter } from '../auth/ws-auth.adapter';
import { ChatMapper } from '../../infras/mapper';
import { WebSocket } from 'ws';
import { IncomingMessage } from 'http';

@Injectable()
@WebSocketGateway(5003, { cors: true })
export class ChatGateway {
  @WebSocketServer() server: WebSocket.Server; // Access the WebSocket server instance
  private connectedUsers = new Map<string, Set<WebSocket>>(); // Store WebSocket instances
  private readonly logger = new Logger(ChatGateway.name);

  constructor(
    private readonly chatService: ChatService,
    private readonly userClientService: UserClientService,
    private readonly wsAuthAdapter: WsAuthAdapter,
  ) {}

  async handleConnection(ws: WebSocket, request: IncomingMessage) {
    this.logger.log(`New connection attempt: ${request.headers['sec-websocket-key']}`);
    try {
      const cookieHeader = request.headers.cookie;
      if (!cookieHeader) {
        this.logger.error('No cookies provided');
        ws.send(JSON.stringify({ event: 'error', data: { message: 'No cookies provided' } }));
        ws.close();
        return;
      }

      const metadata = this.wsAuthAdapter.createMetadataFromCookie(cookieHeader);
      const user = await this.userClientService.getUserById({ metadata });

      if (!user) {
        this.logger.error('Authentication required');
        ws.send(JSON.stringify({ event: 'error', data: { message: 'Authentication required' } }));
        ws.close();
        return;
      }

      // Store userId and roomIds on the WebSocket instance
      (ws as any).userId = user.id;
      (ws as any).roomIds = new Set<string>();

      if (!this.connectedUsers.has(user.id)) {
        this.connectedUsers.set(user.id, new Set<WebSocket>());
      }
      this.connectedUsers.get(user.id)?.add(ws);

      ws.on('message', (data: Buffer) => {
        try {
          const message = JSON.parse(data.toString());
          this.handleMessage(ws, message);
        } catch {
          this.logger.error('Invalid message format');
          ws.send(JSON.stringify({ event: 'error', data: { message: 'Invalid message format' } }));
        }
      });

      ws.on('close', () => this.handleDisconnect(ws));

      ws.send(JSON.stringify({ event: 'connection_ack', data: { status: 'connected' } }));
      this.logger.log(`User ${user.id} connected`);
    } catch (error) {
      this.logger.error(`Connection error: ${error.message}`);
      ws.send(JSON.stringify({ event: 'error', data: { message: 'Connection failed' } }));
      ws.close();
    }
  }

  private handleDisconnect(ws: WebSocket) {
    const userId = (ws as any).userId;
    if (!userId) return;
    this.logger.log(`User ${userId} disconnected`);
    const userSockets = this.connectedUsers.get(userId);
    if (userSockets) {
      userSockets.delete(ws);
      if (userSockets.size === 0) {
        this.connectedUsers.delete(userId);
      }
    }
  }

  private async handleMessage(ws: WebSocket, message: any) {
    const userId = (ws as any).userId;
    if (!userId) return;

    try {
      switch (message.event) {
        case 'joinRoom':
          await this.handleJoinRoom(ws, message.data);
          break;
        case 'sendMessage':
          await this.handleSendMessage(ws, message.data);
          break;
        case 'getRooms':
          try {
            const userId = (ws as any).userId;
            const rooms = await this.chatService.getRoomsByUserId(userId);
            this.logger.log('Rooms fetched for getRooms:', JSON.stringify(rooms, null, 2));
            const roomDomains = (rooms as any[]).map((doc) => ChatMapper.toRoomDomain(doc));
            ws.send(JSON.stringify({ event: 'roomsList', data: roomDomains.map((room) => ChatMapper.toRoomResponse(room)) }));
          } catch (error) {
            this.logger.error(`Failed to fetch rooms: ${error.message}`);
            ws.send(JSON.stringify({ event: 'error', data: { message: 'Failed to fetch rooms' } }));
          }
          break;
        default:
          this.logger.warn(`Unknown event: ${message.event}`);
          ws.send(JSON.stringify({ event: 'error', data: { message: 'Unknown event' } }));
      }
    } catch (error) {
      this.logger.error(`Message processing error: ${error.message}`);
      ws.send(JSON.stringify({ event: 'error', data: { message: 'Failed to process message' } }));
    }
  }

  private async handleJoinRoom(ws: WebSocket, data: { roomId: string }) {
    try {
      const userId = (ws as any).userId;
      const room = await this.chatService.getRoomById(data.roomId);
      if (!room.members.includes(userId)) {
        this.logger.error(`User ${userId} is not a member of room ${data.roomId}`);
        ws.send(JSON.stringify({ event: 'error', data: { message: 'Not a member of this room' } }));
        return;
      }

      (ws as any).roomIds.add(data.roomId);

      const messages = await this.chatService.getMessagesByRoomId(data.roomId, 50);
      const messageResponses = messages.map((msg) => ChatMapper.toMessageResponse(msg));

      ws.send(
        JSON.stringify({
          event: 'messageHistory',
          data: {
            roomId: data.roomId,
            messages: messageResponses,
          },
        }),
      );

      this.broadcastToRoom(
        data.roomId,
        'userJoined',
        {
          roomId: data.roomId,
          userId,
        },
        [userId],
      );
      this.logger.log(`User ${userId} joined room ${data.roomId}`);
    } catch (error) {
      this.logger.error(`Failed to join room: ${error.message}`);
      ws.send(JSON.stringify({ event: 'error', data: { message: 'Failed to join room' } }));
    }
  }

  private async handleSendMessage(ws: WebSocket, data: any) {
    try {
      const userId = (ws as any).userId;
      const message = await this.chatService.createMessage({
        roomId: data.roomId,
        senderId: userId,
        content: data.content,
        type: this.chatService.validateMessageType(data.type),
        replyToId: data.replyToId,
      });

      const messageResponse = ChatMapper.toMessageResponse(message);
      this.broadcastToRoom(data.roomId, 'messageReceived', messageResponse);
      this.logger.log(`Message sent in room ${data.roomId} by user ${userId}`);
    } catch (error) {
      this.logger.error(`Failed to send message: ${error.message}`);
      ws.send(JSON.stringify({ event: 'error', data: { message: 'Failed to send message' } }));
    }
  }

  private broadcastToRoom(roomId: string, event: string, data: unknown, excludeUsers: string[] = []) {
    const message = JSON.stringify({ event, data });
    this.connectedUsers.forEach((sockets, userId) => {
      if (!excludeUsers.includes(userId)) {
        sockets.forEach((ws) => {
          if ((ws as any).roomIds.has(roomId)) {
            try {
              ws.send(message);
            } catch (error) {
              this.logger.error(`Broadcast error to user ${userId}: ${error.message}`);
            }
          }
        });
      }
    });
  }
}
