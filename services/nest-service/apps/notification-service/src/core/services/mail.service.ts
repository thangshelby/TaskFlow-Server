import { UserClientService } from '@nest-service/core';
import { Injectable } from '@nestjs/common';
import { IMailSender } from '@notification-service/core/interfaces/mail-sender.interface';
@Injectable()
export class MailService {
  constructor(
    private readonly userClientService: UserClientService,
    private readonly mailSender: IMailSender,
  ) {}
  async sendVerifyOtp(input: { userId: string; otp?: string }) {
    const { userId, otp } = input;
    if (!otp || !userId) {
      throw new Error('Invalid OTP || User Id');
    }

    const user = await this.userClientService.getUserById({ userId: userId });

    if (!user) {
      throw new Error('User not found!');
    }

    const data = {
      NAME: `${user.firstName} ${user.lastName}`,
      OTP: otp,
      EXPIRE_MINUTES: 5,
      YEAR: 2025,
    };

    await this.mailSender.sendMail({
      subject: 'test',
      to: user.email,
      template: 'verify-otp.hbs',
      data,
    });
  }
}
