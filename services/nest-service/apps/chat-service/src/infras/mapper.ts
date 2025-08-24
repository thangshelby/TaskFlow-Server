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
    let lastMessage: MessageDomain | undefined = undefined;

    if (doc.lastMessage) {
      const messageDoc = doc.lastMessage as any as MessageDocument;

      if (messageDoc._id) {
        lastMessage = {
          id: messageDoc._id.toString(),
          roomId: messageDoc.roomId,
          senderId: messageDoc.senderId,
          content: messageDoc.content,
          type: messageDoc.type as MessageType,
          replyToId: messageDoc.replyToId ?? undefined,
          createdAt: messageDoc.createdAt instanceof Date ? messageDoc.createdAt : new Date(messageDoc.createdAt),
          updatedAt: messageDoc.updatedAt ? (messageDoc.updatedAt instanceof Date ? messageDoc.updatedAt : new Date(messageDoc.updatedAt)) : undefined,
        };
      }
    }

    return {
      id: doc._id.toString(),
      name: doc.name,
      type: doc.type as 'DIRECT' | 'GROUP',
      members: doc.members,
      lastMessage,
      createdAt: doc.createdAt,
      updatedAt: doc.updatedAt,
    };
  }

  static toMessageResponse(message: MessageDomain): MessageResponse {
    // Defensive: ensure id exists
    const id = message.id || (message as any)._id?.toString();
    if (!id) {
      console.warn('No ID found for message, generating temporary one');
      return {
        id: `temp-${Date.now()}`,
        roomId: message.roomId,
        senderId: message.senderId,
        content: '[Unknown]',
        type: message.type || 'TEXT',
        replyToId: message.replyToId,
        createdAt: new Date().toISOString(),
      };
    }

    if (!message.createdAt) {
      console.warn('Missing createdAt in message', id);
      message.createdAt = new Date();
    } else if (!(message.createdAt instanceof Date)) {
      const d = new Date(message.createdAt);
      message.createdAt = isNaN(d.getTime()) ? new Date() : d;
    }

    return {
      id,
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
