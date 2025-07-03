import { Logger } from '@nestjs/common';
import { NestFactory } from '@nestjs/core';
import { ChatServiceModule } from './chat-service.module';
import { WsAdapter } from '@nestjs/platform-ws';

async function bootstrap() {
  try {
    // Create a NestJS application without an HTTP server
    const app = await NestFactory.create(ChatServiceModule, { cors: true });

    // Use the native WebSocket adapter
    app.useWebSocketAdapter(new WsAdapter(app));

    // Initialize the WebSocket server (ChatGateway will use port 5003)
    await app.init();

    Logger.log(`🚀 WebSocket server running on ws://localhost:5003`);
  } catch (error) {
    Logger.error(`Failed to start WebSocket server: ${error.message}`);
    process.exit(1);
  }
}

bootstrap();
