import { KafkaMessage } from 'src/services/kafka.service';

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
export function validateRequiredFields<T>(message: KafkaMessage): {
  isValid: boolean;
  message?: string;
  missingFields?: string[];
} {
  if (!message.data) {
    return {
      isValid: false,
      message: `Missing data for ${message.id} event`,
    };
  }

  const requiredFields = Object.keys(message.data) as (keyof T)[];
  const missingFields = requiredFields.filter((field) => {
    const value = (message.data as T)[field];
    return value === undefined || value === null;
  });

  if (missingFields.length > 0) {
    return {
      isValid: false,
      message: `Missing required fields for ${message.id} event: ${missingFields.join(', ')}`,
      missingFields: missingFields as string[],
    };
  }

  return { isValid: true };
}
