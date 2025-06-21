export interface ClientToServerEvents {
  // Room events
  joinRoom: (roomId: string) => void;
  leaveRoom: (roomId: string) => void;

  // Message events
  sendMessage: (data: {
    roomId: string;
    content: string;
    type: string;
    replyToId?: string;
  }) => void;

  // Typing indicators
  startTyping: (roomId: string) => void;
  stopTyping: (roomId: string) => void;
}

export interface ServerToClientEvents {
  // Message events
  messageReceived: (message: {
    id: string;
    roomId: string;
    senderId: string;
    content: string;
    type: string;
    replyToId?: string;
    createdAt: string;
  }) => void;

  // Room events
  userJoined: (data: { roomId: string; userId: string }) => void;
  userLeft: (data: { roomId: string; userId: string }) => void;

  // Typing events
  userStartedTyping: (data: { roomId: string; userId: string }) => void;
  userStoppedTyping: (data: { roomId: string; userId: string }) => void;

  // Error events
  error: (error: { message: string }) => void;
}
