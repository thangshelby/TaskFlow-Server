import { GlobalHandleErrorInterceptor, HttpAuthInterceptor } from '@nest-service/core';
import { Logger } from '@nestjs/common';
import { NestFactory } from '@nestjs/core';
import { TransformResponseInterceptor } from '@notification-service/adapters/interceptors/snakeCase.interceptor';
import { NotificationModule } from '@notification-service/notification.module';

async function bootstrap() {
  const app = await NestFactory.create(NotificationModule);
  app.enableCors({
    origin: 'http://localhost:5173',
    methods: 'GET,HEAD,PUT,PATCH,POST,DELETE',
    credentials: true,
  });
  app.useGlobalInterceptors(new HttpAuthInterceptor(), new TransformResponseInterceptor(), new GlobalHandleErrorInterceptor());
  await app.listen(5002);
  Logger.log(`🚀 HTTP server running at http://localhost:5002`);
}
bootstrap();
