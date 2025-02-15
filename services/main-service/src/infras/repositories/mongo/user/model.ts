import mongoose, { Schema, Document } from 'mongoose';
import { User } from '~/domain/entities/user';

export interface IUser extends Document {
  _id: mongoose.Types.ObjectId;
  email: string;
  name: string;
  password: string;
  createdAt?: Date;
}

const UserSchema = new Schema<IUser>(
  {
    email: { type: String, required: true, unique: true },
    name: { type: String, required: true },
    password: { type: String, required: true }
  },
  { timestamps: true }
);

export const UserModel = mongoose.model<IUser>('User', UserSchema);

// Convert User schema to User domain
export function toUserDomain(user: IUser): User {
  return new User(
    user._id.toString(), // Convert MongoDB ObjectId to string
    user.name,
    user.email,
    user.createdAt ? user.createdAt.toISOString() : new Date().toISOString()
  );
}
