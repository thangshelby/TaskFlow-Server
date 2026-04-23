import { Controller, Get } from '@nestjs/common';

@Controller('/health')
export class HealthController {
  @Get()
  async healthCheck() {
    return {
      status: 'ok',
      message: 'Notification service is healthy',
      timestamp: new Date().toISOString(),
    };
  }
}
