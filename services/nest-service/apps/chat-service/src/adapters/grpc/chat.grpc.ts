import { Controller } from '@nestjs/common';
import { GrpcMethod } from '@nestjs/microservices';
import { ChatService } from '../../core/services/chat.service';
import { MessageType } from '../../core/models/chat';
import { UserClientService } from '@nest-service/core';
import { Metadata } from '@grpc/grpc-js';

interface SendMessageRequest {
  roomId: string;
  content: string;
  type: string;
  replyToId?: string;
}

interface CreateRoomRequest {
  name?: string;
  type: 'DIRECT' | 'GROUP';
  members: string[];
}

interface GetMessagesRequest {
  roomId: string;
  limit?: number;
  before?: Date;
}

interface AddMemberRequest {
  roomId: string;
  userId: string;
}

@Controller()
export class ChatGrpcController {
  constructor(
    private readonly chatService: ChatService,
    private readonly userClientService: UserClientService,
  ) {}

  @GrpcMethod('chat_service.ChatService', 'SendMessage')
  async sendMessage(data: SendMessageRequest, metadata: Metadata) {
    const user = await this.userClientService.getUserById({ metadata });
    const type = this.chatService.validateMessageType(data.type);

    const message = await this.chatService.createMessage({
      roomId: data.roomId,
      senderId: user.id,
      content: data.content,
      type,
      replyToId: data.replyToId,
    });

    return {
      status: 'success',
      message: 'Message sent successfully',
      data: message,
    };
  }

  @GrpcMethod('chat_service.ChatService', 'CreateRoom')
  async createRoom(data: CreateRoomRequest, metadata: Metadata) {
    const user = await this.userClientService.getUserById({ metadata });
    
    const room = await this.chatService.createRoom({
      name: data.name,
      type: data.type,
      members: [user.id, ...data.members],
    });

    return {
      status: 'success',
      message: 'Room created successfully',
      data: room,
    };
  }

  @GrpcMethod('chat_service.ChatService', 'GetMessages')
  async getMessages(data: GetMessagesRequest) {
    const messages = await this.chatService.getMessagesByRoomId(
      data.roomId,
      data.limit,
      data.before,
    );

    return {
      status: 'success',
      message: 'Messages retrieved successfully',
      data: messages,
    };
  }

  @GrpcMethod('chat_service.ChatService', 'GetUserRooms')
  async getUserRooms(_: any, metadata: Metadata) {
    const user = await this.userClientService.getUserById({ metadata });
    const rooms = await this.chatService.getRoomsByUserId(user.id);

    return {
      status: 'success',
      message: 'Rooms retrieved successfully',
      data: rooms,
    };
  }

  @GrpcMethod('chat_service.ChatService', 'AddMember')
  async addMember(data: AddMemberRequest) {
    const room = await this.chatService.addMemberToRoom(data.roomId, data.userId);

    return {
      status: 'success',
      message: 'Member added successfully',
      data: room,
    };
  }

  @GrpcMethod('chat_service.ChatService', 'RemoveMember')
  async removeMember(data: AddMemberRequest) {
    const room = await this.chatService.removeMemberFromRoom(data.roomId, data.userId);

    return {
      status: 'success',
      message: 'Member removed successfully',
      data: room,
    };
  }
}