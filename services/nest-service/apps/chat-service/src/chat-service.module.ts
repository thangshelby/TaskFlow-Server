import { Module } from '@nestjs/common';
import { MongooseModule } from '@nestjs/mongoose';
import { CoreModule } from '@nest-service/core';
import { ChatGateway } from './adapters/websocket/chat.gateway';
import { ChatService } from './core/services/chat.service';
import { ChatRepository } from './infras/repos/chat.repo';
import { Message, MessageSchema, Room, RoomSchema } from './infras/schema/chat.schema';
import { AuthModule } from './adapters/auth/auth.module';
import { WsAuthAdapter } from './adapters/auth/ws-auth.adapter';

@Module({
  imports: [
    CoreModule,
    AuthModule,
    MongooseModule.forFeature([
      { name: Message.name, schema: MessageSchema },
      { name: Room.name, schema: RoomSchema },
    ]),
  ],
  providers: [ChatGateway, ChatService, ChatRepository, WsAuthAdapter, { provide: 'IChatRepo', useClass: ChatRepository }],
})
export class ChatServiceModule {}
