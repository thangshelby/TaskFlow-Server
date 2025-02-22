import 'reflect-metadata';
import { Entity, PrimaryGeneratedColumn, Column } from 'typeorm';

@Entity()
export class UserModel {
  @PrimaryGeneratedColumn('uuid')
  id: string;

  @Column({ type: 'varchar', length: 255 }) // Explicitly define type
  name: string;

  @Column({ type: 'varchar', unique: true, length: 255 }) // Explicitly define type
  email: string;

  @Column({ type: 'varchar', length: 255 }) // Explicitly define type
  password: string;
}
