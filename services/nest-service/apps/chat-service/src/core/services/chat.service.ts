import { Injectable } from '@nestjs/common';
import { RpcException } from '@nestjs/microservices';
import { status } from '@grpc/grpc-js';
import { IChatRepo } from '../interfaces/chat-repo.interface';
import { MessageDomain, MessageType, RoomDomain } from '../models/chat';

export interface CreateMessageParams {
  roomId: string;
  senderId: string;
  content: string;
  type: MessageType;
  replyToId?: string;
}

export interface CreateRoomParams {
  name?: string;
  type: 'DIRECT' | 'GROUP';
  members: string[];
}

@Injectable()
export class ChatService {
  constructor(private readonly chatRepo: IChatRepo) {}

  async createMessage(params: CreateMessageParams): Promise<MessageDomain> {
    const message = await this.chatRepo.createMessage({
      ...params,
      createdAt: new Date(),
    });

    await this.chatRepo.updateLastMessage(params.roomId, message);
    return message;
  }

  async getMessagesByRoomId(roomId: string, limit = 50, before?: Date): Promise<MessageDomain[]> {
    return this.chatRepo.getMessagesByRoomId(roomId, limit, before);
  }

  async createRoom(params: CreateRoomParams): Promise<RoomDomain> {
    if (params.type === 'DIRECT' && params.members.length !== 2) {
      throw new RpcException({
        code: status.INVALID_ARGUMENT,
        message: 'Direct chat must have exactly 2 members',
      });
    }

    if (params.type === 'GROUP' && params.members.length < 2) {
      throw new RpcException({
        code: status.INVALID_ARGUMENT,
        message: 'Group chat must have at least 2 members',
      });
    }

    return this.chatRepo.createRoom({
      ...params,
      createdAt: new Date(),
    });
  }

  async getRoomById(id: string): Promise<RoomDomain> {
    const room = await this.chatRepo.getRoomById(id);
    if (!room) {
      throw new RpcException({
        code: status.NOT_FOUND,
        message: 'Room not found',
      });
    }
    return room;
  }

  async getRoomsByUserId(userId: string): Promise<RoomDomain[]> {
    return this.chatRepo.getRoomsByUserId(userId);
  }

  async addMemberToRoom(roomId: string, userId: string): Promise<RoomDomain> {
    const room = await this.getRoomById(roomId);
    
    if (room.type === 'DIRECT') {
      throw new RpcException({
        code: status.INVALID_ARGUMENT,
        message: 'Cannot add members to direct chat',
      });
    }

    if (room.members.includes(userId)) {
      throw new RpcException({
        code: status.ALREADY_EXISTS,
        message: 'User is already a member of this room',
      });
    }

    return this.chatRepo.addMemberToRoom(roomId, userId);
  }

  async removeMemberFromRoom(roomId: string, userId: string): Promise<RoomDomain> {
    const room = await this.getRoomById(roomId);
    
    if (room.type === 'DIRECT') {
      throw new RpcException({
        code: status.INVALID_ARGUMENT,
        message: 'Cannot remove members from direct chat',
      });
    }

    if (!room.members.includes(userId)) {
      throw new RpcException({
        code: status.NOT_FOUND,
        message: 'User is not a member of this room',
      });
    }

    if (room.members.length <= 2) {
      throw new RpcException({
        code: status.INVALID_ARGUMENT,
        message: 'Cannot remove member from room with only 2 members',
      });
    }

    return this.chatRepo.removeMemberFromRoom(roomId, userId);
  }

  validateMessageType(type: string): MessageType {
    if (!type || !Object.values(MessageType).includes(type as MessageType)) {
      throw new RpcException({
        code: status.INVALID_ARGUMENT,
        message: 'Invalid message type',
      });
    }
    return type as MessageType;
  }
}