import { Prop, Schema, SchemaFactory } from '@nestjs/mongoose';
import { HydratedDocument } from 'mongoose';
import { MessageType } from '../../core/models/chat';

export type MessageDocument = HydratedDocument<Message>;
export type RoomDocument = HydratedDocument<Room>;

@Schema({ timestamps: true, collection: 'messages' })
export class Message {
  @Prop({ required: true })
  roomId: string;

  @Prop({ required: true })
  senderId: string;

  @Prop({ required: true })
  content: string;

  @Prop({ required: true, enum: MessageType })
  type: string;

  @Prop()
  replyToId?: string;

  @Prop()
  createdAt: Date;

  @Prop()
  updatedAt: Date;
}

@Schema({ timestamps: true, collection: 'rooms' })
export class Room {
  @Prop()
  name?: string;

  @Prop({ required: true, enum: ['DIRECT', 'GROUP'] })
  type: string;

  @Prop({ type: [String], required: true })
  members: string[];

  @Prop({ type: Object })
  lastMessage?: Record<string, any>;

  @Prop()
  createdAt: Date;

  @Prop()
  updatedAt: Date;
}

export const MessageSchema = SchemaFactory.createForClass(Message);
export const RoomSchema = SchemaFactory.createForClass(Room);