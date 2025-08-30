import { KafkaActionType, KafkaMessage, KafkaService } from '@nest-service/core';
import { Injectable, OnModuleInit } from '@nestjs/common';
import { SendMailVerifyOtpMessageData } from '@notification-service/core/models/mail';
import { MailService } from '@notification-service/core/services/mail.service';
import validator from '@notification-service/utils/validate.utils';
import { EachMessagePayload } from 'kafkajs';

export const MAIL_KAFKA_TOPIC = 'mails';

@Injectable()
export class MailSubscriberService implements OnModuleInit {
  constructor(
    private readonly kafkaService: KafkaService,
    private readonly mailService: MailService,
  ) {}

  async onModuleInit(): Promise<void> {
    this.kafkaService.on(MAIL_KAFKA_TOPIC, this.handleMailMessageReceiver.bind(this));
  }

  private async handleMailMessageReceiver(payload: EachMessagePayload): Promise<void> {
    const { message } = payload;
    const value = message.value?.toString();

    try {
      const message: KafkaMessage = value ? JSON.parse(value) : null;

      switch (message.eventType) {
        case KafkaActionType.MAILS_SEND_VERIFY_OTP_USER:
          await this.handleSendMailVerifyOtp(message);
          break;
        default:
          console.error('❌ [MAIL_TOPIC] Unknown event type:', message.eventType);
          break;
      }
    } catch (err) {
      console.error('❌ [MAIL_TOPIC] Failed to process notification message:', err);
    }
  }

  private async handleSendMailVerifyOtp(kafkaMessage: KafkaMessage): Promise<void> {
    const { isValid, message, data: messageData } = validator.validateRequiredFields<SendMailVerifyOtpMessageData>(kafkaMessage);
    if (!isValid || !messageData) {
      console.error('❌ Missing field:', message);
      return;
    }
    const { userId, data } = messageData;

    await this.mailService.sendVerifyOtp({
      userId: userId,
      otp: data.otp,
    });
  }
}
