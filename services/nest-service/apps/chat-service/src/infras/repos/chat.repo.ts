import { Injectable } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { Model } from 'mongoose';
import { ChatMessageData, MessageDomain, RoomDomain } from '../../core/models/chat';

@Injectable()
export class ChatRepository {
  constructor(
    @InjectModel('Message') private readonly messageModel: Model<MessageDomain>,
    @InjectModel('Room') private readonly roomModel: Model<RoomDomain>,
  ) {}

  async createMessage(data: ChatMessageData): Promise<MessageDomain> {
    const message = new this.messageModel({
      ...data,
      createdAt: new Date(),
    });
    return message.save();
  }

  async createRoom(data: { name?: string; type: 'DIRECT' | 'GROUP'; members: string[] }): Promise<RoomDomain> {
    const room = new this.roomModel({
      ...data,
      createdAt: new Date(),
    });
    return room.save();
  }

  async getMessagesByRoomId(roomId: string, limit?: number, before?: Date): Promise<MessageDomain[]> {
    const query = this.messageModel.find({ roomId });

    if (before) {
      query.where('createdAt').lt(before.getTime());
    }

    if (limit) {
      query.limit(limit);
    }

    return query.sort({ createdAt: -1 }).exec();
  }

  async getRoomsByUserId(userId: string): Promise<RoomDomain[]> {
    return this.roomModel.find({ members: userId }).sort({ updatedAt: -1 }).exec();
  }

  async addMemberToRoom(roomId: string, userId: string): Promise<RoomDomain | null> {
    return this.roomModel.findOneAndUpdate({ _id: roomId }, { $addToSet: { members: userId }, updatedAt: new Date() }, { new: true }).exec();
  }

  async removeMemberFromRoom(roomId: string, userId: string): Promise<RoomDomain | null> {
    return this.roomModel.findOneAndUpdate({ _id: roomId }, { $pull: { members: userId }, updatedAt: new Date() }, { new: true }).exec();
  }

  async getRoomById(roomId: string): Promise<RoomDomain | null> {
    return this.roomModel.findById(roomId).exec();
  }
}
