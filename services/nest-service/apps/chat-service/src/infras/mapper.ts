import { MessageDomain, MessageType, RoomDomain } from '../core/models/chat';
import { Message, Room } from './schema/chat.schema';

export class ChatMapper {
  static toMessageDomain(message: Message): MessageDomain {
    return {
      id: message._id?.toString(),
      roomId: message.roomId,
      senderId: message.senderId,
      content: message.content,
      type: message.type as MessageType,
      replyToId: message.replyToId,
      createdAt: message.createdAt,
      updatedAt: message.updatedAt,
    };
  }

  static toRoomDomain(room: Room): RoomDomain {
    return {
      id: room._id?.toString(),
      name: room.name,
      type: room.type as 'DIRECT' | 'GROUP',
      members: room.members,
      lastMessage: room.lastMessage ? this.toMessageDomain(room.lastMessage as unknown as Message) : undefined,
      createdAt: room.createdAt,
      updatedAt: room.updatedAt,
    };
  }

  static toMessageResponse(message: MessageDomain) {
    return {
      id: message.id,
      roomId: message.roomId,
      senderId: message.senderId,
      content: message.content,
      type: message.type,
      replyToId: message.replyToId,
      createdAt: message.createdAt?.toISOString(),
      updatedAt: message.updatedAt?.toISOString(),
    };
  }

  static toRoomResponse(room: RoomDomain) {
    return {
      id: room.id,
      name: room.name,
      type: room.type,
      members: room.members,
      lastMessage: room.lastMessage ? this.toMessageResponse(room.lastMessage) : null,
      createdAt: room.createdAt?.toISOString(),
      updatedAt: room.updatedAt?.toISOString(),
    };
  }
}