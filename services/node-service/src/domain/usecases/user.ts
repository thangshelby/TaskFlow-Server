import { USER_QUEUE, USER_EVENT } from '~/domain/constant/user';
import { User } from '~/domain/entities/user';
import { IMessageQueue } from '~/domain/interfaces/queue';
import { GetListUserParams, GetListUserRes, IUserRepository, UpdateUserReq } from '~/domain/interfaces/user';

export class UserUseCase {
  constructor(
    private userRepository: IUserRepository,
    private msgQueue: IMessageQueue
  ) {}

  async createUser(userData: { email: string; name: string; password: string }): Promise<User> {
    const existingUser = await this.userRepository.findByEmail(userData.email);
    if (existingUser) {
      throw new Error('User already exists');
    }

    const user = new User(
      Date.now().toString(),
      userData.email,
      userData.name,
      userData.password // Trong thực tế nên hash password
    );

    const savedUser = await this.userRepository.save(user);

    // PUSH TO WORKER TO HANDLE SYNC DB WITH ELASTIC
    await this.msgQueue.sendMessage(USER_QUEUE, {
      event: USER_EVENT.CREATE_USER,
      userId: savedUser.id,
      email: savedUser.email,
      name: savedUser.name
    });

    return savedUser;
  }
  async getUser(params: GetListUserParams): Promise<GetListUserRes> {
    return await this.userRepository.getListUser(params.page, params.limit);
  }
  async updateUser(body: UpdateUserReq): Promise<User> {
    return await this.userRepository.updateUser(body);
  }
}
