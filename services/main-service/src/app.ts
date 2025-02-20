import 'dotenv/config';
import express from 'express';
import { UserUseCase } from '~/domain/usecases/user';
import { RabbitMQService } from '~/infras/queue/rabbitmq';
import { MongoUserRepository } from '~/infras/repositories/mongo/user/user';
import { startWorker } from '~/infras/worker/user.worker';
import { UserController } from '~/controllers/user.controller';
import userRoutes from '~/routes/user.routes';
import { AuthController } from '~/controllers/auth.controller';
import authRoutes from '~/routes/auth.routes';
import { connectDB } from '~/infras/repositories/mongo';
// import { RedisService } from '~/infras/cache/redis';
// import { ElasticsearchService } from '~/infras/repositories/elastic';

export const setupApp = async () => {
  const app = express();
  app.use(express.json());

  // Setup MongoDB
  await connectDB();

  // Setup Dependencies
  // const elasticService = new ElasticsearchService();
  // await elasticService.init();

  // const redisService = new RedisService();
  // await redisService.connect();

  const rabbitMQService = new RabbitMQService();
  await rabbitMQService.init();
  const userRepository = new MongoUserRepository();
  // const userRepository = new MySQLUserRepository();

  // Setup UseCase
  const userUsecase = new UserUseCase(userRepository, rabbitMQService);

  // Setup Controller
  const userController = new UserController(userUsecase);
  const authController = new AuthController(userUsecase);

  // Setup Worker
  startWorker(rabbitMQService, userRepository);

  // Setup Routes
  const router = express.Router();
  userRoutes(router, userController);
  authRoutes(router, authController);
  app.use('/api/v1', router);

  return app;
};
