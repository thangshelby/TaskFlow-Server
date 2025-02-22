import { User } from '~/domain/entities/user';
import { GetListUserRes, IUserRepository, UpdateUserReq } from '~/domain/interfaces/user';
import { toUserDomain, UserModel } from '~/infras/repositories/mongo/user/model';
export class MongoUserRepository implements IUserRepository {
  private userModel;

  constructor(userModel = UserModel) {
    this.userModel = userModel;
  }
  async save(user: User): Promise<User> {
    // TODO: convert type of user from domain layer to MondoSchema before save
    const mongoUser = new this.userModel(user);

    await mongoUser.save();
    return user;
  }

  async findByEmail(email: string): Promise<User | null> {
    const user = await this.userModel.findOne({ email });
    if (!user) return null;
    return toUserDomain(user);
  }

  async findById(id: string): Promise<User | null> {
    const user = await this.userModel.findById(id);
    if (!user) return null;
    return new User(user.id, user.email, user.name, user.password);
  }

  async getListUser(page: number, limit: number): Promise<GetListUserRes> {
    const query = this.userModel.find({});

    if (limit > 0) {
      query.skip((page - 1) * limit).limit(limit);
    }

    const [users, total] = await Promise.all([query, this.userModel.countDocuments({})]);

    if (!users) return { data: [], total: 0, totalPages: 0 };

    const totalPages = limit > 0 ? Math.ceil(total / limit) : 1;
    const data = users.map((user) => toUserDomain(user));

    return { data, total, totalPages };
  }
  async updateUser(body: UpdateUserReq): Promise<User> {
    const { id, email, password, name } = body;

    const updatedUser = await this.userModel.findByIdAndUpdate(
      id,
      {
        $set: {
          ...(email && { email: email }),
          ...(password && { password: password }),
          ...(name && { name: name })
        }
      },
      { new: true, runValidators: true }
    );
    if (!updatedUser) {
      throw new Error('User not found');
    }
    return toUserDomain(updatedUser);
  }
}
