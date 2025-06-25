import { NestFactory } from '@nestjs/core';
import { ChatServiceModule } from './chat-service.module';

async function bootstrap() {
  const app = await NestFactory.create(ChatServiceModule);
  await app.listen(5003);
}
bootstrap();
