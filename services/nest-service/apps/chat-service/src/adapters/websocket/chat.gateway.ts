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
        ws.send(
          JSON.stringify({
            event: 'error',
            data: {
              code: 'AUTH_NO_COOKIES',
              message: 'No cookies provided',
              details: {
                requiredHeader: 'cookie',
              },
            },
          }),
        );
        ws.close();
        return;
      }

      const metadata = this.wsAuthAdapter.createMetadataFromCookie(cookieHeader);
      const user = await this.userClientService.getUserById({ metadata });

      if (!user) {
        this.logger.error('Authentication required');
        ws.send(
          JSON.stringify({
            event: 'error',
            data: {
              code: 'AUTH_REQUIRED',
              message: 'Authentication required',
              details: {
                reason: 'User not found',
              },
            },
          }),
        );
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

      ws.send(
        JSON.stringify({
          event: 'connection_ack',
          data: {
            status: 'connected',
            user: {
              id: user.id,
              firstName: user.firstName,
              lastName: user.lastName,
              email: user.email,
              role: user.role,
              connectionId: request.headers['sec-websocket-key'],
            },
          },
        }),
      );
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
          await this.handleJoinRoom(ws, { roomId: message.data });
          break;
        case 'sendMessage':
          await this.handleSendMessage(ws, message.data);
          break;
        case 'getRooms':
          await this.handleGetRooms(ws);
          // try {
          //   const userId = (ws as any).userId;
          //   const rooms = await this.chatService.getRoomsByUserId(userId);
          //   this.logger.log('Rooms fetched for getRooms:', JSON.stringify(rooms, null, 2));
          //   const roomDomains = (rooms as any[]).map((doc) => ChatMapper.toRoomDomain(doc));

          //   const roomResponses = roomDomains.map((room) => ChatMapper.toRoomResponse(room));

          //   ws.send(
          //     JSON.stringify({
          //       event: 'roomsList',
          //       data: {
          //         rooms: roomResponses,
          //         timestamp: new Date().toISOString(),
          //         totalCount: roomResponses.length,
          //         userId,
          //       },
          //     }),
          //   );
          // } catch (error) {
          //   this.logger.error(`Failed to fetch rooms: ${error.message}`);
          //   ws.send(
          //     JSON.stringify({
          //       event: 'error',
          //       data: {
          //         code: 'ROOMS_FETCH_FAILED',
          //         message: 'Failed to fetch rooms',
          //         details: {
          //           userId: (ws as any).userId,
          //           error: error.message,
          //         },
          //       },
          //     }),
          //   );
          // }
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

      // First get the room to validate access
      const existingRoom = await this.chatService.getRoomById(data.roomId);

      if (!existingRoom || !existingRoom.members.includes(userId)) {
        this.logger.error(`User ${userId} is not a member of room ${data.roomId}`);
        ws.send(
          JSON.stringify({
            event: 'error',
            data: {
              code: 'ROOM_ACCESS_DENIED',
              message: 'Not a member of this room',
              details: {
                roomId: data.roomId,
                userId,
              },
            },
          }),
        );
        return;
      }

      (ws as any).roomIds.add(data.roomId);

      //Map Messages
      const messages = await this.chatService.getMessagesByRoomId(data.roomId, 50);
      this.logger.log(`Found ${messages.length} messages for room ${data.roomId}`);
      if (existingRoom.lastMessage) {
        this.logger.log('LastMessage id:', existingRoom.lastMessage.id);
        this.logger.log('LastMessage createdAt:', existingRoom.lastMessage.createdAt);
        this.logger.log('Is createdAt a Date?', existingRoom.lastMessage.createdAt instanceof Date);
        this.logger.log('Is createdAt valid?', !isNaN(existingRoom.lastMessage.createdAt.getTime()));
      }
      const roomResponse = ChatMapper.toRoomResponse(existingRoom);
      const messageResponses = messages.map((msg) => ({
        id: msg.id,
        roomId: msg.roomId,
        senderId: msg.senderId,
        content: msg.content,
        type: msg.type,
        replyToId: msg.replyToId,
        createdAt: msg.createdAt.toISOString(),
        updatedAt: msg.updatedAt?.toISOString(),
      }));

      // Send room details and message history to joining user
      ws.send(
        JSON.stringify({
          event: 'roomJoined',
          data: {
            room: roomResponse,
            messages: messageResponses,
            joinedAt: new Date().toISOString(),
          },
        }),
      );

      // Broadcast to other room members
      this.broadcastToRoom(
        data.roomId,
        'userJoined',
        {
          roomId: data.roomId,
          userId,
          timestamp: new Date().toISOString(),
          userCount: existingRoom.members.length,
        },
        [userId],
      );
      this.logger.log(`User ${userId} joined room ${data.roomId}`);
    } catch (error) {
      this.logger.error(`Failed to join room: ${error.message}`);
      ws.send(
        JSON.stringify({
          event: 'error',
          data: {
            code: 'ROOM_JOIN_FAILED',
            message: 'Failed to join room',
            details: {
              roomId: data.roomId,
              error: error.message,
            },
          },
        }),
      );
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

      // Send acknowledgment to sender
      ws.send(
        JSON.stringify({
          event: 'messageSent',
          data: {
            message: messageResponse,
            status: 'delivered',
            timestamp: new Date().toISOString(),
          },
        }),
      );

      // Broadcast to other room members
      this.broadcastToRoom(data.roomId, 'messageReceived', {
        ...messageResponse,
        timestamp: new Date().toISOString(),
      });

      this.logger.log(`Message sent in room ${data.roomId} by user ${userId}`);
    } catch (error) {
      this.logger.error(`Failed to send message: ${error.message}`);
      ws.send(
        JSON.stringify({
          event: 'error',
          data: {
            code: 'MESSAGE_SEND_FAILED',
            message: 'Failed to send message',
            details: {
              roomId: data.roomId,
              error: error.message,
            },
          },
        }),
      );
    }
  }
  private async handleGetRooms(ws: WebSocket) {
    try {
      const userId = (ws as any).userId;
      const rooms = await this.chatService.getRoomsByUserId(userId);
      this.logger.log('Rooms fetched for getRooms:', JSON.stringify(rooms, null, 2));
      const roomDomains = (rooms as any[]).map((doc) => ChatMapper.toRoomDomain(doc));

      const roomResponses = roomDomains.map((room) => ChatMapper.toRoomResponse(room));

      ws.send(
        JSON.stringify({
          event: 'roomsList',
          data: {
            rooms: roomResponses,
            timestamp: new Date().toISOString(),
            totalCount: roomResponses.length,
            userId,
          },
        }),
      );
    } catch (error) {
      this.logger.error(`Failed to fetch rooms: ${error.message}`);
      ws.send(
        JSON.stringify({
          event: 'error',
          data: {
            code: 'ROOMS_FETCH_FAILED',
            message: 'Failed to fetch rooms',
            details: {
              userId: (ws as any).userId,
              error: error.message,
            },
          },
        }),
      );
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
