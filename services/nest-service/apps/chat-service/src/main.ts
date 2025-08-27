import { Logger } from '@nestjs/common';
import { NestFactory } from '@nestjs/core';
import { ChatServiceModule } from './chat-service.module';
import { WsAdapter } from '@nestjs/platform-ws';

async function bootstrap() {
  try {
    const start = Date.now();
    Logger.log(`[BOOT] Starting bootstrap at ${new Date().toISOString()}`);

    // Create a NestJS application without an HTTP server
    const beforeCreate = Date.now();
    Logger.log(`[BOOT] Before NestFactory.create: ${beforeCreate - start}ms since start`);
    const app = await NestFactory.create(ChatServiceModule, { cors: true });
    Logger.log(`[BOOT] After NestFactory.create: ${Date.now() - start}ms since start`);

    // Use the native WebSocket adapter
    app.useWebSocketAdapter(new WsAdapter(app));

    // Initialize the WebSocket server (ChatGateway will use port 5003)
    const beforeInit = Date.now();
    Logger.log(`[BOOT] Before app.init: ${beforeInit - start}ms since start`);
    await app.init();
    Logger.log(`[BOOT] After app.init: ${Date.now() - start}ms since start`);

    Logger.log(`🚀 WebSocket server running on ws://localhost:5003`);
  } catch (error) {
    Logger.error(`Failed to start WebSocket server: ${error.message}`);
    process.exit(1);
  }
}

bootstrap();
