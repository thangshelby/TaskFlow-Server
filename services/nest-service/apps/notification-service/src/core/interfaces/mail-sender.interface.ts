export interface SendMailOptions {
  to: string;
  subject: string;
  text?: string;
  html?: string;
  template?: string;
  data?: any;
}

export abstract class IMailSender {
  abstract sendMail(options: SendMailOptions): Promise<void>;
}
