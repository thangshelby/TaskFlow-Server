import { UserRes } from '@nest-service/core/types/base';

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

  /**
   * Sprint bắt đầu
   */
  SPRINT_STARTED = 'SPRINT_STARTED',

  /**
   * Được thêm vào dự án
   */
  PROJECT_ADDED = 'PROJECT_ADDED',

  /**
   * Được thêm vào team dự án
   */
  PROJECT_TEAM_ADDED = 'PROJECT_TEAM_ADDED',
}
export enum ReferenceType {
  ISSUE = 'issue',
  PROJECT = 'project',
  COMMENT = 'comment',
  SPRINT = 'sprint',
  SYSTEM = 'system',
  PROJECT_MEMBER = 'project_member',
}
export interface NotificationDomain {
  id?: string;
  recipientId: string;
  actorId?: string;
  type: NotificationType;
  referenceId?: string;
  referenceType?: ReferenceType;
  content?: string;
  isRead: boolean;
  createdAt: string | Date;
  referenceData?: any;
  recipient?: UserRes;
  actor?: UserRes;
}
export interface NotificationMessageData {
  recipientId: string;
  actorId?: string;
  issueId?: string;
  type: string;
}
