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
          let messageData: any;
          try {
            if (!message.Body) {
              continue;
            }

            const body = message.Body.trim();
            // console.log('DEBUG: Received SQS message body:', body);

            let parsedBody: any;
            try {
              parsedBody = JSON.parse(body);
            } catch (e) {
              // Robust check: handle common issue with missing braces in this environment
              if (body.includes('"id":') && !body.startsWith('{')) {
                const fixedBody = `{${body}${body.endsWith('}') ? '' : '}'}`;
                try {
                  parsedBody = JSON.parse(fixedBody);
                  this.logService.warn(`Auto-fixed SQS message body missing braces for message ${message.MessageId}`);
                } catch (e2) {
                  throw new Error(`Failed to parse SQS body even after auto-fix attempt. Original: ${body.substring(0, 100)}...`);
                }
              } else {
                throw e;
              }
            }

            // Check if it's an SNS envelope
            // SNS shape: { "Type": "Notification", "Message": "{\"id\":...}", ... }
            if (parsedBody && typeof parsedBody === 'object' && 'Message' in parsedBody) {
              const innerMessage = parsedBody.Message;
              if (typeof innerMessage === 'string') {
                try {
                  // Attempt to parse the inner message if it's a JSON string
                  messageData = JSON.parse(innerMessage);
                } catch (e) {
                  // If inner Message is not a JSON string, use it as is
                  messageData = innerMessage;
                }
              } else {
                messageData = innerMessage;
              }
            } else {
              // Not an SNS envelope or no Message field, use the parsed body directly
              messageData = parsedBody;
            }
          } catch (error) {
            this.logService.error(`Failed to parse SQS message body: ${message.MessageId}. Body snippet: ${message.Body?.substring(0, 100)}`, (error as Error).stack);
            continue;
          }

          try {
            await callback(messageData);

            if (message.ReceiptHandle) {
              await this.sqsClient.send(
                new DeleteMessageCommand({
                  QueueUrl: queueUrl,
                  ReceiptHandle: message.ReceiptHandle,
                }),
              );
            }
          } catch (error: any) {
            this.logService.error(`Error processing message ${message.MessageId} from queue ${queueUrl}`, error?.stack);
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
