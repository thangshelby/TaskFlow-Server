import { MessageType } from '../../core/models/chat';
import { Socket } from 'socket.io';

// Message response type
export interface MessageResponse {
  // Message must have an id when emitted to clients
  id: string;
  roomId: string;
  senderId: string;
  content: string;
  type: MessageType;
  replyToId?: string;
  createdAt: string;
}

export interface RoomResponse {
  id: string;
  name?: string;
  type: 'DIRECT' | 'GROUP';
  members: string[];
  lastMessage?: MessageResponse;
  createdAt: string;
}

export interface ClientToServerEvents {
  // Room events
  joinRoom: (roomId: string) => void;
  leaveRoom: (roomId: string) => void;

  // Message events
  sendMessage: (data: { roomId: string; content: string; type: string; replyToId?: string }) => void;

  // Typing indicators
  startTyping: (roomId: string) => void;
  stopTyping: (roomId: string) => void;
}

export interface ServerToClientEvents {
  // Message events
  messageReceived: (message: MessageResponse) => void;

  // Room events
  userJoined: (data: { roomId: string; userId: string }) => void;
  userLeft: (data: { roomId: string; userId: string }) => void;
  roomCreated: (room: RoomResponse) => void;
  memberAdded: (data: { roomId: string; userId: string }) => void;
  memberRemoved: (data: { roomId: string; userId: string }) => void;

  // Typing events
  userStartedTyping: (data: { roomId: string; userId: string }) => void;
  userStoppedTyping: (data: { roomId: string; userId: string }) => void;

  // Error events
  error: (error: { message: string }) => void;
}

// Custom socket type with auth token
export interface AuthenticatedSocket extends Socket<ClientToServerEvents, ServerToClientEvents> {
  handshake: {
    auth?: {
      token?: string;
    };
  } & Socket['handshake'];
}
