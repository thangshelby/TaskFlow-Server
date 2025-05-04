import { ApplicationConfiguration } from './applicationConfiguration';

export abstract class ApplicationService {
  async start(): Promise<void> {}

  async stop(): Promise<void> {}

  static isEnabled(cfg: ApplicationConfiguration): boolean {
    return true;
  }
}
