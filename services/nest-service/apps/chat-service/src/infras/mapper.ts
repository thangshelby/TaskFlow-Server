import { MessageDomain, RoomDomain, MessageType } from '../core/models/chat';
import { MessageDocument, RoomDocument } from './schema/chat.schema';
import { MessageResponse, RoomResponse } from '../adapters/websocket/chat.events';

export class ChatMapper {
  static toMessageDomain(doc: MessageDocument): MessageDomain {
    return {
      id: doc._id.toString(),
      roomId: doc.roomId,
      senderId: doc.senderId,
      content: doc.content,
      type: doc.type as MessageType,
      replyToId: doc.replyToId,
      createdAt: doc.createdAt,
      updatedAt: doc.updatedAt,
    };
  }

  static toRoomDomain(doc: RoomDocument): RoomDomain {
    return {
      id: doc._id.toString(),
      name: doc.name,
      type: doc.type as 'DIRECT' | 'GROUP',
      members: doc.members,
      lastMessage: doc.lastMessage ? this.toMessageDomain(doc.lastMessage as MessageDocument) : undefined,
      createdAt: doc.createdAt,
      updatedAt: doc.updatedAt,
    };
  }

  static toMessageResponse(message: MessageDomain): MessageResponse {
    if (!message.id || !message.createdAt) {
      throw new Error('Message must have an id and createdAt timestamp');
    }
    return {
      id: message.id,
      roomId: message.roomId,
      senderId: message.senderId,
      content: message.content,
      type: message.type,
      replyToId: message.replyToId,
      createdAt: message.createdAt.toISOString(),
    };
  }

  static toRoomResponse(room: RoomDomain): RoomResponse {
    if (!room.id) {
      throw new Error('Room must have an id');
    }
    return {
      id: room.id,
      name: room.name,
      type: room.type,
      members: room.members,
      lastMessage: room.lastMessage ? this.toMessageResponse(room.lastMessage) : undefined,
      createdAt: (room.createdAt || new Date()).toISOString(),
    };
  }
}
