import 'reflect-metadata';
import { DataSource } from 'typeorm';
import { UserModel } from '~/infras/repositories/mysql/user/model';
export const AppDataSource = new DataSource({
  type: process.env.DB_TYPE as 'mysql',
  host: process.env.DB_HOST,
  port: Number(process.env.DB_PORT),
  username: process.env.DB_USER,
  password: process.env.DB_PASS,
  database: process.env.DB_NAME,
  entities: [UserModel],
  synchronize: true
});
