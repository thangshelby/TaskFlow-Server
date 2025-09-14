/* eslint-disable */
import { WebSocketGateway, WebSocketServer } from '@nestjs/websockets';
import { Injectable, Logger } from '@nestjs/common';
import { ChatService } from '../../core/services/chat.service';
import { UserClientService } from '@nest-service/core';
import { WsAuthAdapter } from '../auth/ws-auth.adapter';
import { ChatMapper } from '../../infras/mapper';
import { WebSocket } from 'ws';
import { IncomingMessage } from 'http';
import * as jwt from 'jsonwebtoken';
@Injectable()
@WebSocketGateway({
  cors: {
    origin: ['http://localhost:5173', 'http://localhost:3000'],
    credentials: true,
  },
  path: '/ws',
})
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
      ws.send(
        JSON.stringify({
          event: 'connection_ack',
          data: { status: 'connected', userId: user.id },
        }),
      );
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

          // Handle authentication
          if (message.event === 'authenticate') {
            const token = message.data?.token;
            if (!token) {
              ws.send(JSON.stringify({ event: 'error', data: { message: 'Auth token required' } }));
              ws.close();
              return;
            }

            try {
              const decoded = jwt.verify(token, 'your-secret') as any;
              (ws as any).userId = decoded.userId;
              ws.send(JSON.stringify({ event: 'connection_ack', data: { status: 'connected' } }));
            } catch (err) {
              ws.send(JSON.stringify({ event: 'error', data: { message: 'Invalid token' } }));
              ws.close();
            }
            return;
          }

          // Only proceed if authenticated
          if (!(ws as any).userId) {
            ws.send(JSON.stringify({ event: 'error', data: { message: 'Authenticate first' } }));
            return;
          }

          // Handle other events
          this.handleMessage(ws, message);
        } catch (err) {
          ws.send(JSON.stringify({ event: 'error', data: { message: 'Invalid message' } }));
        }
      });

      ws.on('close', () => this.handleDisconnect(ws));

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

    if (!message.event) {
      ws.send(
        JSON.stringify({
          event: 'error',
          data: { message: 'Missing event type' },
        }),
      );
      return;
    }

    try {
      switch (message.event) {
        case 'joinRoom': {
          const { roomId } = message.data || {};
          if (!roomId) {
            ws.send(
              JSON.stringify({
                event: 'error',
                data: { message: 'roomId is required to join room' },
              }),
            );
            return;
          }
          await this.handleJoinRoom(ws, { roomId });
          break;
        }

        case 'sendMessage': {
          const { roomId, content, type, replyToId } = message.data || {};
          if (!roomId || !content) {
            ws.send(
              JSON.stringify({
                event: 'error',
                data: { message: 'roomId and content are required' },
              }),
            );
            return;
          }
          await this.handleSendMessage(ws, { roomId, content, type, replyToId });
          break;
        }

        case 'getRooms': {
          await this.handleGetRooms(ws);
          break;
        }

        case 'getMessageHistory': {
          const { roomId, before, limit } = message.data || {};
          if (!roomId) {
            ws.send(
              JSON.stringify({
                event: 'error',
                data: { message: 'roomId is required for message history' },
              }),
            );
            return;
          }
          await this.handleGetMessageHistory(ws, { roomId, before, limit });
          break;
        }

        default:
          this.logger.warn(`Unknown event: ${message.event}`);
          ws.send(
            JSON.stringify({
              event: 'error',
              data: { message: `Unknown event: ${message.event}` },
            }),
          );
      }
    } catch (error) {
      this.logger.error(`Error handling event ${message.event}: ${error.message}`);
      ws.send(
        JSON.stringify({
          event: 'error',
          data: { message: 'Internal server error' },
        }),
      );
    }
  }

  private async handleJoinRoom(ws: WebSocket, data: { roomId: string }) {
    const { roomId } = data;
    try {
      const userId = (ws as any).userId;

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
      const messages = await this.chatService.getMessagesByRoomId(data.roomId, 50);
      this.logger.log(`Found ${messages.length} messages for room ${data.roomId}`);
      if (existingRoom.lastMessage) {
        this.logger.log('LastMessage id:', existingRoom.lastMessage.id);
        this.logger.log('LastMessage createdAt:', existingRoom.lastMessage.createdAt);
        this.logger.log('Is createdAt a Date?', existingRoom.lastMessage.createdAt instanceof Date);
        this.logger.log('Is createdAt valid?', !isNaN(existingRoom.lastMessage.createdAt.getTime()));
      }
      const roomResponse = ChatMapper.toRoomResponse(existingRoom);
      const messageResponses = await Promise.all(
        messages.map(async (msg) => {
          const sender = await this.userClientService.getUserById({ userId: msg.senderId });

          const senderName = sender?.firstName && sender?.lastName ? `${sender.firstName} ${sender.lastName}` : sender?.email || `User ${msg.senderId.slice(0, 8)}`;

          return {
            id: msg.id,
            roomId: msg.roomId,
            senderId: msg.senderId,
            content: msg.content,
            type: msg.type,
            replyToId: msg.replyToId,
            createdAt: msg.createdAt.toISOString(),
            updatedAt: msg.updatedAt?.toISOString(),
            senderName,
          };
        }),
      );

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

      const sender = await this.userClientService.getUserById({ userId });

      const senderName = sender?.firstName && sender?.lastName ? `${sender.firstName} ${sender.lastName}` : sender?.email || `User ${userId.slice(0, 8)}`;

      const messageResponse = {
        ...ChatMapper.toMessageResponse(message),
        senderName,
      };

      ws.send(
        JSON.stringify({
          event: 'messageSent',
          data: { message: messageResponse },
        }),
      );

      this.broadcastToRoom(data.roomId, 'messageReceived', {
        ...messageResponse,
      });
    } catch (error) {
      ws.send(
        JSON.stringify({
          event: 'error',
          data: { message: 'Failed to send message' },
        }),
      );
    }
  }
  private async handleGetRooms(ws: WebSocket) {
    try {
      const userId = (ws as any).userId;

      const rooms = await this.chatService.getRoomsByUserId(userId);

      const roomResponses = await Promise.all(
        rooms.map(async (roomDoc: any) => {
          const room = ChatMapper.toRoomDomain(roomDoc);
          const roomResponse = ChatMapper.toRoomResponse(room);

          if (roomResponse.lastMessage) {
            try {
              const sender = await this.userClientService.getUserById({
                userId: roomResponse.lastMessage.senderId,
              });

              const senderName = sender?.firstName && sender?.lastName ? `${sender.firstName} ${sender.lastName}` : sender?.email || 'User';

              // ✅ Safe: check exists, then add senderName
              (roomResponse.lastMessage as any).senderName = senderName;
            } catch (err) {
              // ✅ Safe: check before assign
              if (roomResponse.lastMessage) {
                (roomResponse.lastMessage as any).senderName = 'Unknown';
              }
            }
          }

          return roomResponse;
        }),
      );

      // ✅ Fixed: Use `data` field
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
      const userId = (ws as any)?.userId;
      this.logger.error(`Failed to fetch rooms: ${error.message}`);
      ws.send(
        JSON.stringify({
          event: 'error',
          data: {
            code: 'ROOMS_FETCH_FAILED',
            message: 'Failed to fetch rooms',
            details: {
              userId: userId,
              error: error.message,
            },
          },
        }),
      );
    }
  }
  private async handleGetMessageHistory(ws: WebSocket, data: { roomId: string; before?: string; limit?: number }) {
    const userId = (ws as any).userId;
    const { roomId, before, limit = 30 } = data;

    try {
      // Verify user is member of room
      const room = await this.chatService.getRoomById(roomId);
      if (!room || !room.members.includes(userId)) {
        return ws.send(
          JSON.stringify({
            event: 'error',
            data: { message: 'Access denied to room' },
          }),
        );
      }

      // ✅ Convert 'before' string to Date object if provided
      const beforeDate: Date | undefined = before ? new Date(before) : undefined;

      // Validate date if provided
      if (before && (isNaN(beforeDate!.getTime()) || beforeDate!.toString() === 'Invalid Date')) {
        return ws.send(
          JSON.stringify({
            event: 'error',
            data: { message: 'Invalid "before" timestamp' },
          }),
        );
      }

      // Fetch messages
      const messages = await this.chatService.getMessagesByRoomId(roomId, limit, beforeDate);
      const messageResponses = await Promise.all(
        messages.map(async (msg) => {
          const sender = await this.userClientService.getUserById({ userId: msg.senderId });
          const senderName = sender?.firstName && sender?.lastName ? `${sender.firstName} ${sender.lastName}` : sender?.email || `User ${msg.senderId.slice(0, 8)}`;

          return {
            id: msg.id,
            roomId: msg.roomId,
            senderId: msg.senderId,
            content: msg.content,
            type: msg.type,
            replyToId: msg.replyToId,
            createdAt: msg.createdAt.toISOString(),
            updatedAt: msg.updatedAt?.toISOString(),
            senderName, // ✅ Add senderName
          };
        }),
      );

      ws.send(
        JSON.stringify({
          event: 'messageHistory',
          data: {
            roomId,
            messages: messageResponses,
          },
        }),
      );
    } catch (error) {
      ws.send(
        JSON.stringify({
          event: 'error',
          data: { message: 'Failed to load message history' },
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
