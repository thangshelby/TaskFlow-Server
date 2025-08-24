// /* eslint-disable */
// import { WebSocketGateway, WebSocketServer, OnGatewayConnection, OnGatewayDisconnect } from '@nestjs/websockets';
// import { Injectable, Logger } from '@nestjs/common';
// import { ChatService } from '../../core/services/chat.service';
// import { UserClientService } from '@nest-service/core';
// import { WsAuthAdapter } from '../auth/ws-auth.adapter';
// import { ChatMapper } from '../../infras/mapper';
// import { Server, Socket } from 'socket.io';
// import { RoomDomain, MessageDomain, MessageType } from '../../core/models/chat';

// @Injectable()
// @WebSocketGateway(5003, {
//   cors: {
//     origin: ['http://localhost:5173', 'http://localhost:5003'],
//     credentials: true,
//     methods: ['GET', 'POST'],
//   },
// })
// export class ChatGateway implements OnGatewayConnection, OnGatewayDisconnect {
//   @WebSocketServer() server: Server;
//   private connectedUsers = new Map<string, Set<Socket>>();
//   private readonly logger = new Logger(ChatGateway.name);

//   constructor(
//     private readonly chatService: ChatService,
//     private readonly userClientService: UserClientService,
//     private readonly wsAuthAdapter: WsAuthAdapter,
//   ) {}

//   async handleConnection(socket: Socket) {
//     this.logger.log(`New connection attempt: ${socket?.id || 'unknown'}`);
//     if (!socket || !socket.handshake) {
//       this.logger.error('Invalid socket object or missing handshake');
//       return;
//     }

//     try {
//       const cookieHeader = socket.handshake.headers.cookie;
//       const authHeader = socket.handshake.headers.authorization;
//       this.logger.log(`Cookie: ${cookieHeader || 'none'}, Authorization: ${authHeader || 'none'}`);

//       if (!cookieHeader && !authHeader) {
//         this.logger.error('No cookies or authorization header provided');
//         socket.emit('error', { message: 'No cookies or authorization header provided' });
//         socket.disconnect();
//         return;
//       }

//       const metadata = this.wsAuthAdapter.createMetadataFromCookie(cookieHeader || '');
//       this.logger.log(`Metadata from WsAuthAdapter: ${JSON.stringify(metadata)}`);
//       const user = await this.userClientService.getUserById({ metadata });
//       this.logger.log(`UserClientService response: ${JSON.stringify(user)}`);

//       if (!user || !user.id) {
//         this.logger.error('Authentication failed: No valid user found');
//         socket.emit('error', { message: 'Authentication required' });
//         socket.disconnect();
//         return;
//       }

//       socket.data.userId = user.id;
//       socket.data.roomIds = new Set<string>();

//       if (!this.connectedUsers.has(user.id)) {
//         this.connectedUsers.set(user.id, new Set<Socket>());
//       }
//       this.connectedUsers.get(user.id)?.add(socket);

//       socket.on('getRooms', () => this.handleGetRooms(socket));
//       socket.on('joinRoom', (data) => this.handleJoinRoom(socket, data));
//       socket.on('sendMessage', (data) => this.handleSendMessage(socket, data));
//       socket.on('createRoom', (data) => this.handleCreateRoom(socket, data));

//       socket.emit('connection_ack', { status: 'connected' });
//       this.logger.log(`User ${user.id} connected`);
//     } catch (error) {
//       this.logger.error(`Connection error: ${error.message}, Stack: ${error.stack}`);
//       if (socket && typeof socket.emit === 'function') {
//         socket.emit('error', { message: `Connection failed: ${error.message}` });
//       }
//       if (socket && typeof socket.disconnect === 'function') {
//         socket.disconnect();
//       } else {
//         this.logger.error('Cannot disconnect: socket.disconnect is not a function');
//       }
//     }
//   }

//   handleDisconnect(socket: Socket) {
//     if (!socket || !socket.data) {
//       this.logger.error('Invalid socket object in handleDisconnect');
//       return;
//     }
//     const userId = socket.data.userId;
//     if (!userId) return;
//     this.logger.log(`User ${userId} disconnected`);
//     const userSockets = this.connectedUsers.get(userId);
//     if (userSockets) {
//       userSockets.delete(socket);
//       if (userSockets.size === 0) {
//         this.connectedUsers.delete(userId);
//       }
//     }
//   }

//   private async handleGetRooms(socket: Socket) {
//     if (!socket || !socket.data) {
//       this.logger.error('Invalid socket object in handleGetRooms');
//       return;
//     }
//     const userId = socket.data.userId;
//     if (!userId) {
//       this.logger.error('No userId found for socket');
//       socket.emit('error', { message: 'Authentication required' });
//       return;
//     }

//     try {
//       this.logger.log(`Processing getRooms for user ${userId}`);
//       const rooms = await this.chatService.getRoomsByUserId(userId);
//       this.logger.log(`Rooms fetched for user ${userId}: ${JSON.stringify(rooms, null, 2)}`);
//       if (!Array.isArray(rooms)) {
//         this.logger.error(`Invalid rooms data for user ${userId}: ${JSON.stringify(rooms)}`);
//         socket.emit('error', { message: 'Invalid rooms data' });
//         return;
//       }
//       const roomResponses = rooms.map((room) => ChatMapper.toRoomResponse(room));
//       this.logger.log(`Sending roomsList to user ${userId}: ${JSON.stringify(roomResponses, null, 2)}`);
//       socket.emit('roomsList', roomResponses);
//       if (roomResponses.length === 0) {
//         this.logger.warn(`No rooms found for user ${userId}`);
//       }
//     } catch (error) {
//       this.logger.error(`Failed to fetch rooms for user ${userId}: ${error.message}`);
//       socket.emit('error', { message: `Failed to fetch rooms: ${error.message}` });
//     }
//   }

//   private async handleCreateRoom(socket: Socket, data: { name: string; type: 'DIRECT' | 'GROUP'; members: string[] }) {
//     if (!socket || !socket.data) {
//       this.logger.error('Invalid socket object in handleCreateRoom');
//       return;
//     }
//     try {
//       const userId = socket.data.userId;
//       if (!userId) {
//         this.logger.error('No userId found for socket');
//         socket.emit('error', { message: 'Authentication required' });
//         return;
//       }
//       if (!data.name || !data.type || !data.members.includes(userId)) {
//         this.logger.error(`Invalid createRoom data from user ${userId}: ${JSON.stringify(data)}`);
//         socket.emit('error', { message: 'Invalid room data' });
//         return;
//       }

//       const room: RoomDomain = {
//         id: '', // ID will be set by repository
//         name: data.name,
//         type: data.type,
//         members: data.members,
//         createdAt: new Date(),
//         updatedAt: new Date(),
//       };

//       const createdRoom = await this.chatService.createRoom(room);
//       if (!createdRoom.id) {
//         this.logger.error(`Created room has no id: ${JSON.stringify(createdRoom)}`);
//         socket.emit('error', { message: 'Failed to create room: No ID' });
//         return;
//       }

//       const roomResponse = ChatMapper.toRoomResponse(createdRoom);
//       socket.emit('roomCreated', roomResponse);
//       this.logger.log(`Room ${createdRoom.id} created by user ${userId}`);
//       this.broadcastToRoom(createdRoom.id, 'roomCreated', roomResponse, [userId]);
//     } catch (error) {
//       this.logger.error(`Failed to create room: ${error.message}`);
//       socket.emit('error', { message: `Failed to create room: ${error.message}` });
//     }
//   }

//   private async handleJoinRoom(socket: Socket, data: { roomId: string }) {
//     if (!socket || !socket.data) {
//       this.logger.error('Invalid socket object in handleJoinRoom');
//       return;
//     }
//     try {
//       const userId = socket.data.userId;
//       if (!userId) {
//         this.logger.error('No userId found for socket');
//         socket.emit('error', { message: 'Authentication required' });
//         return;
//       }
//       const room = await this.chatService.getRoomById(data.roomId);
//       if (!room.members.includes(userId)) {
//         this.logger.error(`User ${userId} is not a member of room ${data.roomId}`);
//         socket.emit('error', { message: 'Not a member of this room' });
//         return;
//       }

//       socket.data.roomIds.add(data.roomId);

//       const messages = await this.chatService.getMessagesByRoomId(data.roomId, 50);
//       const messageResponses = messages.map((msg) => ChatMapper.toMessageResponse(msg));

//       socket.emit('messageHistory', {
//         roomId: data.roomId,
//         messages: messageResponses,
//       });

//       this.broadcastToRoom(
//         data.roomId,
//         'userJoined',
//         {
//           roomId: data.roomId,
//           userId,
//         },
//         [userId],
//       );
//       this.logger.log(`User ${userId} joined room ${data.roomId}`);
//     } catch (error) {
//       this.logger.error(`Failed to join room: ${error.message}`);
//       socket.emit('error', { message: 'Failed to join room' });
//     }
//   }

//   private async handleSendMessage(socket: Socket, data: { roomId: string; content: string; type: string; replyToId?: string }) {
//     if (!socket || !socket.data) {
//       this.logger.error('Invalid socket object in handleSendMessage');
//       return;
//     }
//     try {
//       const userId = socket.data.userId;
//       if (!userId) {
//         this.logger.error('No userId found for socket');
//         socket.emit('error', { message: 'Authentication required' });
//         return;
//       }
//       const message: MessageDomain = {
//         id: '',
//         roomId: data.roomId,
//         senderId: userId,
//         content: data.content,
//         type: this.chatService.validateMessageType(data.type),
//         replyToId: data.replyToId,
//         createdAt: new Date(),
//         updatedAt: new Date(),
//       };

//       const createdMessage = await this.chatService.createMessage(message);
//       const messageResponse = ChatMapper.toMessageResponse(createdMessage);
//       this.broadcastToRoom(data.roomId, 'messageReceived', messageResponse);
//       this.logger.log(`Message sent in room ${data.roomId} by user ${userId}`);
//     } catch (error) {
//       this.logger.error(`Failed to send message: ${error.message}`);
//       socket.emit('error', { message: 'Failed to send message' });
//     }
//   }

//   private broadcastToRoom(roomId: string, event: string, data: unknown, excludeUsers: string[] = []) {
//     this.connectedUsers.forEach((sockets, userId) => {
//       if (!excludeUsers.includes(userId)) {
//         sockets.forEach((socket) => {
//           if (socket.data.roomIds.has(roomId)) {
//             try {
//               socket.emit(event, data);
//             } catch (error) {
//               this.logger.error(`Broadcast error to user ${userId}: ${error.message}`);
//             }
//           }
//         });
//       }
//     });
//   }
// }
