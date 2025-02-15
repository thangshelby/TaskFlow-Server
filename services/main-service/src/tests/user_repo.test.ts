import { MySQLUserRepository } from '~/infras/repositories/mysql/user/user';
import { AppDataSource } from '~/infras/repositories/mysql';
import { User } from '~/domain/entities/user';
import { UpdateUserReq } from '~/domain/interfaces/user';
// import { RedisService } from '~/infras/cache/redis';

beforeAll(async () => {
  await AppDataSource.initialize(); // Connect to DB before tests
});

afterAll(async () => {
  await AppDataSource.destroy(); // Close DB connection after tests
});

describe('MySQLUserRepository', () => {
  let userRepo: MySQLUserRepository;
  let testUser: User;
  // const redisService = new RedisService();
  // redisService.connect();

  beforeEach(async () => {
    userRepo = new MySQLUserRepository();
    const email = 'testuser@example.com';
    testUser = new User('12345', email, 'Test User', 'SecurePass123');
    testUser = await userRepo.save(testUser); // Save a user before each test
  });

  test('Should find a user by ID', async () => {
    const foundUser = await userRepo.findById(testUser.id);
    expect(foundUser).not.toBeNull();
    expect(foundUser?.id).toBe(testUser.id);
    expect(foundUser?.email).toBe(testUser.email);
  });

  test('Should find a user by email', async () => {
    const foundUser = await userRepo.findByEmail(testUser.email);
    expect(foundUser).not.toBeNull();
    expect(foundUser?.email).toBe(testUser.email);
  });

  test('Should return a paginated list of users', async () => {
    const page = 1;
    const limit = 10;
    const result = await userRepo.getListUser(page, limit);

    expect(result).toHaveProperty('data');
    expect(result).toHaveProperty('total');
    expect(result).toHaveProperty('totalPages');
    expect(Array.isArray(result.data)).toBe(true);
  });

  test('Should update a user', async () => {
    const updateData: UpdateUserReq = {
      id: testUser.id,
      name: 'Updated User',
      email: 'updated@example.com'
    };

    const updatedUser = await userRepo.updateUser(updateData);
    expect(updatedUser.name).toBe(updateData.name);
    expect(updatedUser.email).toBe(updateData.email);
  });

  test('Should throw error when updating a non-existent user', async () => {
    const updateData: UpdateUserReq = {
      id: 'non-existent-id',
      name: 'Fake User',
      email: 'fake@example.com'
    };

    await expect(userRepo.updateUser(updateData)).rejects.toThrow('User not found');
  });
});
