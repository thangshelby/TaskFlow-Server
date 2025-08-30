export interface SendMailVerifyOtpMessageData {
  userId: string;
  data: {
    otp?: string;
  };
}
