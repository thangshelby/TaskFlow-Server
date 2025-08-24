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
    // Debug log
    console.log('Querying messages for roomId:', roomId);

    const query = this.messageModel.find({ roomId });

    if (before) {
      query.where('createdAt').lt(before.getTime());
    }

    if (limit) {
      query.limit(limit);
    }

    const results = await query.sort({ createdAt: -1 }).lean().exec();

    // Transform the documents to include id instead of _id
    const messages = results.map((doc) => ({
      id: doc._id.toString(),
      roomId: doc.roomId,
      senderId: doc.senderId,
      content: doc.content,
      type: doc.type,
      replyToId: doc.replyToId,
      createdAt: doc.createdAt,
      updatedAt: doc.updatedAt,
    }));

    console.log(
      'Messages found:',
      messages.length,
      messages.map((m) => m.id),
    );

    return messages;
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
  async updateLastMessage(roomId: string, message: MessageDomain): Promise<RoomDomain | null> {
    return this.roomModel.findByIdAndUpdate(roomId, { lastMessage: message, updatedAt: new Date() }, { new: true }).exec();
  }
}
