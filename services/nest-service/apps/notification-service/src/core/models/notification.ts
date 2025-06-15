export interface NotificationDomain {
  recipientId: string;
  actorId?: string;
  type: string;
  referenceId?: string;
  referenceType?: string;
  content: string;
  isRead: boolean;
  createdAt: Date;
}
export interface UserCreatedData {
  userId: string;
  name: string;
}
