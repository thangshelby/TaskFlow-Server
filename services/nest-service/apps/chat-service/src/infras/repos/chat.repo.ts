import { Injectable } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { Model } from 'mongoose';
import { ChatMessageData, MessageDomain, RoomDomain } from '../../core/models/chat';

@Injectable()
export class ChatRepository {
  logger: any;
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
    console.log('Querying messages for roomId:', roomId);

    const query = this.messageModel.find({ roomId });

    if (before) {
      query.where('createdAt').lt(before.getTime()); // ✅ Date -> number
    }

    if (limit) {
      query.limit(limit);
    }

    const results = await query.sort({ createdAt: -1 }).lean().exec();

    // Step 1: Map and validate — only return MessageDomain or null
    const candidates = results.map((doc): MessageDomain | null => {
      const id = doc._id?.toString();
      const createdAt = doc.createdAt ? new Date(doc.createdAt) : null;

      // ✅ Validate required fields
      if (!id || !createdAt || isNaN(createdAt.getTime())) {
        console.warn('Invalid message skipped:', { _id: doc._id, createdAt: doc.createdAt });
        return null;
      }

      // ✅ Construct object that *exactly* matches MessageDomain
      const message: MessageDomain = {
        id,
        roomId: doc.roomId,
        senderId: doc.senderId,
        content: doc.content,
        type: doc.type,
        replyToId: doc.replyToId ?? undefined,
        createdAt,
        updatedAt: doc.updatedAt ? new Date(doc.updatedAt) : undefined,
      };

      return message;
    });

    // Step 2: Filter out nulls with correct type guard
    const messages = candidates.filter((msg): msg is MessageDomain => msg !== null);

    // Step 3: Safe to use .map(m => m.id)
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
