export enum MessageType {
  TEXT = 'TEXT',
  IMAGE = 'IMAGE',
  FILE = 'FILE',
  SYSTEM = 'SYSTEM'
}

export interface MessageDomain {
  id?: string;
  roomId: string;
  senderId: string;
  content: string;
  type: MessageType;
  replyToId?: string;
  createdAt: Date;
  updatedAt?: Date;
}

export interface RoomDomain {
  id?: string;
  name?: string;
  type: 'DIRECT' | 'GROUP';
  members: string[];
  lastMessage?: MessageDomain;
  createdAt: Date;
  updatedAt?: Date;
}

export interface ChatMessageData {
  roomId: string;
  senderId: string;
  content: string;
  type: MessageType;
  replyToId?: string;
}