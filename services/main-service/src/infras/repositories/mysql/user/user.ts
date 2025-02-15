import { User } from '~/domain/entities/user';
import { IUserRepository, GetListUserRes, UpdateUserReq } from '~/domain/interfaces/user';
// import { RedisService } from '~/infras/cache/redis';

import { AppDataSource } from '~/infras/repositories/mysql';
import { UserModel } from '~/infras/repositories/mysql/user/model';
export class MySQLUserRepository implements IUserRepository {
  private userRepo = AppDataSource.getRepository(UserModel);
  // private redisService: RedisService;

  // constructor(redisService: RedisService) {
  //   this.redisService = redisService;
  // }

  async save(user: User): Promise<User> {
    return await this.userRepo.save(user);
  }

  async findById(id: string): Promise<User | null> {
    return await this.userRepo.findOne({ where: { id } });
  }

  async findByEmail(email: string): Promise<User | null> {
    return await this.userRepo.findOne({ where: { email } });
  }

  async getListUser(page: number, limit: number): Promise<GetListUserRes> {
    // const cacheKey = `users:${page}:${limit}`;
    // const cachedData = await this.redisService.get(cacheKey);

    // if (cachedData) {
    //   return cachedData;
    // }

    const [data, total] = await this.userRepo.findAndCount({
      skip: (page - 1) * limit,
      take: limit
    });
    const result = {
      data,
      total,
      totalPages: Math.ceil(total / limit)
    };

    // await this.redisService.set(cacheKey, result, 60);
    return result;
  }

  async updateUser(body: UpdateUserReq): Promise<User> {
    const { id, ...updateFields } = body;

    await this.userRepo.update(id, updateFields);
    const updatedUser = await this.findById(id);

    if (!updatedUser) {
      throw new Error('User not found');
    }

    return updatedUser;
  }
}
