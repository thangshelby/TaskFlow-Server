import { Prop, Schema, SchemaFactory } from '@nestjs/mongoose';
import { HydratedDocument } from 'mongoose';

export type NotificationDocument = HydratedDocument<Notification>;

@Schema({ timestamps: { createdAt: 'createdAt' }, collection: 'notifications' })
export class INotification {
  @Prop({ required: true })
  recipientId: string;

  @Prop()
  actorId?: string;

  @Prop({ required: true })
  type: string;

  @Prop()
  referenceId?: string;

  @Prop()
  referenceType?: string;

  @Prop({ required: true })
  content: string;

  @Prop({ default: false })
  isRead: boolean;

  @Prop()
  createdAt: Date;
}

export const NotificationSchema = SchemaFactory.createForClass(INotification);
