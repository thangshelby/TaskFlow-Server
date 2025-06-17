export enum NotificationType {
  /**
   * Khi người dùng được gán vào một task/issue
   */
  ASSIGNMENT = 'ASSIGNMENT',

  /**
   * Khi người dùng được nhắc đến trong bình luận
   */
  MENTION = 'MENTION',

  /**
   * Khi có bình luận mới trong một task mà bạn theo dõi
   */
  COMMENT = 'COMMENT',

  /**
   * Khi trạng thái của task thay đổi
   */
  STATUS_UPDATE = 'STATUS_UPDATE',

  /**
   * Nhắc nhở đến hạn task
   */
  DUE_DATE_REMINDER = 'DUE_DATE_REMINDER',

  /**
   * Khi được mời tham gia một project
   */
  PROJECT_INVITATION = 'PROJECT_INVITATION',

  /**
   * Ai đó thả emoji vào comment của bạn
   */
  REACTION = 'REACTION',

  /**
   * Cảnh báo từ hệ thống (ví dụ: lỗi, bảo trì, ...)
   */
  SYSTEM_ALERT = 'SYSTEM_ALERT',
}
export interface NotificationDomain {
  recipientId: string;
  actorId?: string;
  type: NotificationType;
  referenceId?: string;
  referenceType?: string;
  content: string;
  isRead: boolean;
  createdAt: Date;
}
export interface UserAssignmentData {
  recipientId: string;
  actorId: string;
  issueId: string;
  type: string;
}
