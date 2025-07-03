import { MessageType } from '../../core/models/chat';
import { Socket } from 'socket.io';

// Message response type
export interface MessageResponse {
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

export interface ErrorResponse {
  message: string;
}

export interface ClientToServerEvents {
  // Room events
  joinRoom: (roomId: string) => void;
  leaveRoom: (roomId: string) => void;

  // Message events
  sendMessage: (data: { roomId: string; content: string; type: string; replyToId?: string }) => void;
  getMessageHistory: (data: { roomId: string; before?: string; limit?: number }) => void;

  // Typing indicators
  startTyping: (roomId: string) => void;
  stopTyping: (roomId: string) => void;
}

export interface ServerToClientEvents {
  // Connection events
  connection_ack: (data: { status: string }) => void;

  // Message events
  messageReceived: (message: MessageResponse) => void;
  messageHistory: (data: { roomId: string; messages: MessageResponse[] }) => void;

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
  error: (error: ErrorResponse) => void;
}

export interface InterServerEvents {
  ping: () => void;
}

export interface SocketData {
  userId: string;
}

export type TypedSocket = Socket<ClientToServerEvents, ServerToClientEvents, InterServerEvents, SocketData>;

// Custom socket type with auth data
export interface AuthenticatedSocket extends TypedSocket {
  data: SocketData;
}

// Helper function to check if socket is authenticated
export function isAuthenticated(socket: TypedSocket): socket is AuthenticatedSocket {
  return socket.data && typeof socket.data.userId === 'string';
}
