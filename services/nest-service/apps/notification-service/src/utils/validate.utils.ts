import { KafkaMessage } from '@nest-service/core';

type ValidationResult<T> = {
  isValid: boolean;
  message: string;
  data?: T;
  missingFields?: string[];
};

const validator = {
  validateRequiredFields<T>(message: KafkaMessage): ValidationResult<T> {
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

    return {
      isValid: true,
      message: 'valid',
      data: message.data as T,
    };
  },
};
export default validator;
