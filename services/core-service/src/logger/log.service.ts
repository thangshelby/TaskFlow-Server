// src/log.service.ts
import { Injectable } from '@nestjs/common';
import pino, { Logger as PinoLogger } from 'pino';
import { LogConfiguration } from 'src/applicationConfiguration';

@Injectable()
export class LogService {
  private logger: PinoLogger;

  constructor(private readonly config: LogConfiguration) {}

  async start(): Promise<LogService> {
    this.logger = pino({
      level: this.config?.level || 'info',
      transport: {
        target: 'pino-pretty',
        options: {
          colorize: true,
          translateTime: 'SYS:standard',
        },
      },
    });

    this.logger.info('[LogService] Logger initialized');
    return this;
  }

  info(message: string, ...args: any[]) {
    this.logger.info(message, ...args);
  }

  warn(message: string, ...args: any[]) {
    this.logger.warn(message, ...args);
  }

  error(message: string | Error, ...args: any[]) {
    this.logger.error(
      message instanceof Error ? message.stack : message,
      ...args,
    );
  }

  debug(message: string, ...args: any[]) {
    this.logger.debug(message, ...args);
  }
}
