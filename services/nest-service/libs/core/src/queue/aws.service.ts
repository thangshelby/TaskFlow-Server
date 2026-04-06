import { Injectable, OnModuleDestroy } from '@nestjs/common';
import { DeleteMessageCommand, Message, ReceiveMessageCommand, SQSClient } from '@aws-sdk/client-sqs';
import { ConfigService } from '@nestjs/config';
import { LogService } from '../log/log.service';

type SqsMessageCallback = (message: Message) => Promise<void>;

@Injectable()
export class AwsService implements OnModuleDestroy {
  private readonly sqsClient: SQSClient;
  private readonly pollingFlags = new Map<string, boolean>();
  private readonly runningPollers = new Map<string, Promise<void>>();
  private readonly credentialErrorLogged = new Set<string>();

  constructor(
    private readonly logService: LogService,
    private readonly configService: ConfigService,
  ) {
    const region = this.configService.get<string>('AWS_REGION', 'ap-southeast-1');
    const accessKeyId = this.configService.get<string>('AWS_ACCESS_KEY_ID');
    const secretAccessKey = this.configService.get<string>('AWS_SECRET_ACCESS_KEY');

    this.sqsClient =
      accessKeyId && secretAccessKey
        ? new SQSClient({ region, credentials: { accessKeyId, secretAccessKey } })
        : new SQSClient({ region });
  }

  async onModuleDestroy() {
    for (const queueUrl of this.pollingFlags.keys()) {
      this.pollingFlags.set(queueUrl, false);
    }

    await Promise.allSettled(this.runningPollers.values());
    this.runningPollers.clear();
    this.pollingFlags.clear();
    this.logService.log('SQS consumers disconnected');
  }

  async subscribe(queueUrl: string, callback: SqsMessageCallback): Promise<void> {
    if (!queueUrl) {
      throw new Error('queueUrl is required');
    }

    if (this.pollingFlags.get(queueUrl)) {
      this.logService.log(`Already subscribed to queue: ${queueUrl}`);
      return;
    }

    this.pollingFlags.set(queueUrl, true);
    const poller = this.runPollingLoop(queueUrl, callback);
    this.runningPollers.set(queueUrl, poller);
    this.logService.log(`Subscribed to SQS queue: ${queueUrl}`);
  }

  on(queueUrl: string, callback: SqsMessageCallback): void {
    void this.subscribe(queueUrl, callback);
  }

  private async runPollingLoop(queueUrl: string, callback: SqsMessageCallback): Promise<void> {
    while (this.pollingFlags.get(queueUrl)) {
      try {
        const response = await this.sqsClient.send(
          new ReceiveMessageCommand({
            QueueUrl: queueUrl,
            MaxNumberOfMessages: 1,
            WaitTimeSeconds: 20,
            VisibilityTimeout: 30,
          }),
        );

        if (!response.Messages || response.Messages.length === 0) {
          continue;
        }

        for (const message of response.Messages) {
          const messageBody = message.Body ? JSON.parse(message.Body) : null;
          try {
            await callback(messageBody.Message);

            if (message.ReceiptHandle) {
              await this.sqsClient.send(
                new DeleteMessageCommand({
                  QueueUrl: queueUrl,
                  ReceiptHandle: message.ReceiptHandle,
                }),
              );
            }
          } catch (error: any) {
            this.logService.error(`Error processing message from queue ${queueUrl}`, error?.stack);
          }
        }
      } catch (error: any) {
        if (this.isCredentialExpiredError(error)) {
          if (!this.credentialErrorLogged.has(queueUrl)) {
            this.credentialErrorLogged.add(queueUrl);
            this.logService.error(
              `AWS credentials expired while polling ${queueUrl}. Re-authenticate (for example: aws sso login --profile <profile>) and polling will resume automatically.`,
              error?.stack,
            );
          }

          await this.sleep(15000);
          continue;
        }

        this.credentialErrorLogged.delete(queueUrl);
        this.logService.error(`Error polling queue ${queueUrl}`, error?.stack);
        await this.sleep(1000);
      }
    }

    this.credentialErrorLogged.delete(queueUrl);
    this.runningPollers.delete(queueUrl);
  }

  private isCredentialExpiredError(error: unknown): boolean {
    const name = (error as { name?: string })?.name?.toLowerCase() ?? '';
    const message = (error as { message?: string })?.message?.toLowerCase() ?? '';

    return (
      name.includes('credentialsprovidererror') ||
      message.includes('session has expired') ||
      message.includes('security token included in the request is expired') ||
      message.includes('invalidclienttokenid') ||
      message.includes('expiredtoken')
    );
  }

  private sleep(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
}
