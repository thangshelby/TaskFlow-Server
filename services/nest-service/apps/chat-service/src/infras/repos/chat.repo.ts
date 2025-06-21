import { Injectable } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { Model } from 'mongoose';
import { IChatRepo } from '../../core/interfaces/chat-repo.interface';
import { MessageDomain, RoomDomain } from '../../core/models/chat';
import { Message, Room } from '../schema/chat.schema';

@Injectable()
export class ChatRepository implements IChatRepo {
  constructor(
    @InjectModel(Message.name) private messageModel: Model<Message>,
    @InjectModel(Room.name) private roomModel: Model<Room>,
  ) {}

  async createMessage(message: MessageDomain): Promise<MessageDomain> {
    const newMessage = new this.messageModel(message);
    return await newMessage.save();
  }

  async getMessagesByRoomId(roomId: string, limit = 50, before?: Date): Promise<MessageDomain[]> {
    const query: any = { roomId };
    if (before) {
      query.createdAt = { $lt: before };
    }

    return this.messageModel
      .find(query)
      .sort({ createdAt: -1 })
      .limit(limit)
      .exec();
  }

  async updateMessage(id: string, content: string): Promise<MessageDomain> {
    return this.messageModel.findByIdAndUpdate(
      id,
      { content, updatedAt: new Date() },
      { new: true }
    ).exec();
  }

  async deleteMessage(id: string): Promise<void> {
    await this.messageModel.findByIdAndDelete(id).exec();
  }

  async createRoom(room: RoomDomain): Promise<RoomDomain> {
    const newRoom = new this.roomModel(room);
    return await newRoom.save();
  }

  async getRoomById(id: string): Promise<RoomDomain> {
    return this.roomModel.findById(id).exec();
  }

  async getRoomsByUserId(userId: string): Promise<RoomDomain[]> {
    return this.roomModel.find({ members: userId }).exec();
  }

  async addMemberToRoom(roomId: string, userId: string): Promise<RoomDomain> {
    return this.roomModel.findByIdAndUpdate(
      roomId,
      { $addToSet: { members: userId } },
      { new: true }
    ).exec();
  }

  async removeMemberFromRoom(roomId: string, userId: string): Promise<RoomDomain> {
    return this.roomModel.findByIdAndUpdate(
      roomId,
      { $pull: { members: userId } },
      { new: true }
    ).exec();
  }

  async updateLastMessage(roomId: string, message: MessageDomain): Promise<RoomDomain> {
    return this.roomModel.findByIdAndUpdate(
      roomId,
      { lastMessage: message },
      { new: true }
    ).exec();
  }
}