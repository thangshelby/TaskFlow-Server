export const NOTIFICATION_KAFKA_TOPIC = 'notifications';

export const NotificationAction = {
  USER_CREATED: 'USER_CREATED_ACTION',
  ORDER_PLACED: 'ORDER_PLACED_ACTION',
  PAYMENT_RECEIVED: 'PAYMENT_RECEIVED_ACTION',
} as const;

export type NotificationActionType = (typeof NotificationAction)[keyof typeof NotificationAction];
