// import { Controller, Inject } from '@nestjs/common';
// import { GrpcMethod } from '@nestjs/microservices';
// import { ChatService } from '../../core/services/chat.service';
// import { UserClientService } from '@nest-service/core';
// import { Metadata } from '@grpc/grpc-js';
// import { ChatGateway } from '../websocket/chat.gateway';
// import { ChatMapper } from '../../infras/mapper';
// import { RpcException } from '@nestjs/microservices';
// import { status } from '@grpc/grpc-js';

// interface SendMessageRequest {
//   roomId: string;
//   content: string;
//   type: string;
//   replyToId?: string;
// }

// interface CreateRoomRequest {
//   name?: string;
//   type: 'DIRECT' | 'GROUP';
//   members: string[];
// }

// interface GetMessagesRequest {
//   roomId: string;
//   limit?: number;
//   before?: Date;
// }

// interface AddMemberRequest {
//   roomId: string;
//   userId: string;
// }

// @Controller()
// export class ChatGrpcController {
//   constructor(
//     private readonly chatService: ChatService,
//     private readonly userClientService: UserClientService,
//     private readonly gateway: ChatGateway,
//   ) {}

//   @GrpcMethod('chat_service.ChatService', 'SendMessage')
//   async sendMessage(data: SendMessageRequest, metadata: Metadata) {
//     const user = await this.userClientService.getUserById({ metadata });
//     if (!user) {
//       throw new RpcException({
//         code: status.UNAUTHENTICATED,
//         message: 'User not authenticated',
//       });
//     }
//     const type = this.chatService.validateMessageType(data.type);

//     const message = await this.chatService.createMessage({
//       roomId: data.roomId,
//       senderId: user.id,
//       content: data.content,
//       type,
//       replyToId: data.replyToId,
//     });

//     // Broadcast message to WebSocket clients
//     const messageResponse = ChatMapper.toMessageResponse(message);
//     this.gateway.server.to(data.roomId).emit('messageReceived', messageResponse);

//     return {
//       status: 'success',
//       message: 'Message sent successfully',
//       data: messageResponse,
//     };
//   }

//   @GrpcMethod('chat_service.ChatService', 'CreateRoom')
//   async createRoom(data: CreateRoomRequest, metadata: Metadata) {
//     const user = await this.userClientService.getUserById({ metadata });
//     if (!user) {
//       throw new RpcException({
//         code: status.UNAUTHENTICATED,
//         message: 'User not authenticated',
//       });
//     }

//     const room = await this.chatService.createRoom({
//       name: data.name,
//       type: data.type,
//       members: [user.id, ...data.members],
//     });

//     // Convert to RoomResponse before notifying members
//     const roomResponse = ChatMapper.toRoomResponse(room);

//     // Notify room members via WebSocket
//     room.members.forEach((memberId) => {
//       this.gateway.notifyUserOfNewRoom(memberId, room);
//     });

//     return {
//       status: 'success',
//       message: 'Room created successfully',
//       data: roomResponse,
//     };
//   }

//   @GrpcMethod('chat_service.ChatService', 'GetMessages')
//   async getMessages(data: GetMessagesRequest) {
//     const messages = await this.chatService.getMessagesByRoomId(data.roomId, data.limit, data.before);

//     const messageResponses = messages.map((msg) => ChatMapper.toMessageResponse(msg));

//     return {
//       status: 'success',
//       message: 'Messages retrieved successfully',
//       data: messageResponses,
//     };
//   }

//   @GrpcMethod('chat_service.ChatService', 'GetUserRooms')
//   async getUserRooms(_: any, metadata: Metadata) {
//     const user = await this.userClientService.getUserById({ metadata });
//     if (!user) {
//       throw new RpcException({
//         code: status.UNAUTHENTICATED,
//         message: 'User not authenticated',
//       });
//     }
//     const rooms = await this.chatService.getRoomsByUserId(user.id);
//     const roomResponses = rooms.map((room) => ChatMapper.toRoomResponse(room));

//     return {
//       status: 'success',
//       message: 'Rooms retrieved successfully',
//       data: roomResponses,
//     };
//   }

//   @GrpcMethod('chat_service.ChatService', 'AddMember')
//   async addMember(data: AddMemberRequest) {
//     const room = await this.chatService.addMemberToRoom(data.roomId, data.userId);

//     // Notify room members of the new member
//     this.gateway.server.to(data.roomId).emit('memberAdded', {
//       roomId: data.roomId,
//       userId: data.userId,
//     });

//     // Add the new member to the room's socket group
//     this.gateway.addUserToRoom(data.userId, data.roomId);

//     return {
//       status: 'success',
//       message: 'Member added successfully',
//       data: ChatMapper.toRoomResponse(room),
//     };
//   }

//   @GrpcMethod('chat_service.ChatService', 'RemoveMember')
//   async removeMember(data: AddMemberRequest) {
//     const room = await this.chatService.removeMemberFromRoom(data.roomId, data.userId);

//     // Notify room members of the removal
//     this.gateway.server.to(data.roomId).emit('memberRemoved', {
//       roomId: data.roomId,
//       userId: data.userId,
//     });

//     // Remove user from the room's socket group
//     this.gateway.removeUserFromRoom(data.userId, data.roomId);

//     return {
//       status: 'success',
//       message: 'Member removed successfully',
//       data: ChatMapper.toRoomResponse(room),
//     };
//   }
// }
