import { User } from '~/domain/entities/user';

export interface IUserRepository {
  save(user: User): Promise<User>;
  findByEmail(email: string): Promise<User | null>;
  findById(id: string): Promise<User | null>;
  getListUser(page: number, limit: number): Promise<GetListUserRes>;
  updateUser(body: UpdateUserReq): Promise<User>;
}
export interface GetListUserParams {
  page: number;
  limit: number;
}
export interface GetListUserRes {
  data: User[];
  total: number;
  totalPages: number;
}
export interface UpdateUserReq {
  id: string;
  email?: string;
  name?: string;
  password?: string;
}
export interface UpdateUserRes {
  data: User;
}
