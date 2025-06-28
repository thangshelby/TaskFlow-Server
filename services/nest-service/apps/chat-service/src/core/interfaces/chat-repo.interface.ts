import { MessageDomain, RoomDomain } from '../models/chat';

export abstract class IChatRepo {
  // Message operations
  abstract createMessage(message: MessageDomain): Promise<MessageDomain>;
  abstract getMessagesByRoomId(roomId: string, limit?: number, before?: Date): Promise<MessageDomain[]>;
  abstract updateMessage(id: string, content: string): Promise<MessageDomain>;
  abstract deleteMessage(id: string): Promise<void>;

  // Room operations
  abstract createRoom(room: RoomDomain): Promise<RoomDomain>;
  abstract getRoomById(id: string): Promise<RoomDomain>;
  abstract getRoomsByUserId(userId: string): Promise<RoomDomain[]>;
  abstract addMemberToRoom(roomId: string, userId: string): Promise<RoomDomain>;
  abstract removeMemberFromRoom(roomId: string, userId: string): Promise<RoomDomain>;
  abstract updateLastMessage(roomId: string, message: MessageDomain): Promise<RoomDomain>;
}
