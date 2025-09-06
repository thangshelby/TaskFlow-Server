import { Logger } from '@nestjs/common';
import { NestFactory } from '@nestjs/core';
import { ChatServiceModule } from './chat-service.module';
import { WsAdapter } from '@nestjs/platform-ws';

async function bootstrap() {
  let serverInstance = null;

  if (serverInstance) return;

  try {
    const app = await NestFactory.create(ChatServiceModule, { cors: true });
    app.setGlobalPrefix('api/v1');
    app.useWebSocketAdapter(new WsAdapter(app)); // ← Must be called before listen()

    const port = 5003;
    serverInstance = await app.listen(port);

    Logger.log(`🚀 Server running on http://localhost:${port}`);
    Logger.log(`💡 WebSocket available at ws://localhost:${port}/ws`);
  } catch (error) {
    Logger.error(`Failed to start: ${error.message}`, error.stack);
    process.exit(1);
  }
}
bootstrap();
