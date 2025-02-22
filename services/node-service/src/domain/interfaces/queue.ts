export interface IMessageQueue {
  sendMessage(queue: string, message: object): Promise<void>;
}
