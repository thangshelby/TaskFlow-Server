import { GlobalHandleErrorInterceptor, HttpAuthInterceptor } from '@nest-service/core';
import { Logger } from '@nestjs/common';
import { NestFactory } from '@nestjs/core';
import { TransformResponseInterceptor } from '@notification-service/adapters/interceptors/snakeCase.interceptor';
import { NotificationModule } from '@notification-service/notification.module';

async function bootstrap() {
  const app = await NestFactory.create(NotificationModule);

  // Enable CORS if needed for frontend communication
  app.enableCors();
  app.useGlobalInterceptors(new HttpAuthInterceptor(), new TransformResponseInterceptor(), new GlobalHandleErrorInterceptor());
  await app.listen(5002);
  Logger.log(`🚀 HTTP server running at http://localhost:5002`);
}
bootstrap();
