import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { MongooseModule } from '@nestjs/mongoose';
import { CoreModule } from '@nest-service/core';

// 🔧 Add missing imports:
import { MessageSchema, RoomSchema } from './infras/schema/chat.schema';
import { ChatRepository } from './infras/repos/chat.repo';
import { ChatGrpcController } from './adapters/grpc/chat.grpc';
import { ChatService } from './core/services/chat.service';
import { ChatGateway } from './adapters/websocket/chat.gateway';
@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
      envFilePath: ['.env'],
    }),
    CoreModule,
    // Register both Message and Room schemas:
    MongooseModule.forFeature([
      { name: 'Message', schema: MessageSchema },
      { name: 'Room', schema: RoomSchema },
    ]),
  ],
  controllers: [ChatGrpcController],
  providers: [
    ChatService,
    ChatGateway,
    {
      provide: 'IChatRepo',
      useClass: ChatRepository,
    },
  ],
})
export class ChatServiceModule {}
