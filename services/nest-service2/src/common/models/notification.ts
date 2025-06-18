export interface INotification {
  id: string;
  userId: string;
  eventType: string;
  message: string;
  createdAt: Date;
}
export interface UserCreatedData {
  userId: string;
  name: string;
}
