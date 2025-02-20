import 'dotenv/config';
import express from 'express';
import { UserUseCase } from '~/domain/usecases/user';
// import { RedisService } from '~/infras/cache/redis';
import { RabbitMQService } from '~/infras/queue/rabbitmq';
// import { ElasticsearchService } from '~/infras/repositories/elastic';
// import { connectDB } from '~/infras/repositories/mongo';
// import { MongoUserRepository } from '~/infras/repositories/mongo/user/user';
import { AppDataSource } from '~/infras/repositories/mysql';
import { MySQLUserRepository } from '~/infras/repositories/mysql/user/user';
import { startWorker } from '~/infras/worker/user.worker';
import { UserController } from '~/presentation/controllers/user.controller';
import userRoutes from '~/routes/user.routes';
export const setupApp = async () => {
  const app = express();
  console.log(process.env.DB_PASS);
  app.use(express.json());
  // Setup MongoDB
  // await connectDB();
  AppDataSource.initialize()
    .then(async () => {
      console.log('✅ Connection initialized with database...');
    })
    .catch((error) => console.log(error));

  // Setup Dependencies
  // const elasticService = new ElasticsearchService();
  // await elasticService.init();

  // const redisService = new RedisService();
  // await redisService.connect();

  const rabbitMQService = new RabbitMQService();
  await rabbitMQService.init();
  // const userRepository = new MongoUserRepository();
  const userRepository = new MySQLUserRepository();
  const createUserUseCase = new UserUseCase(userRepository, rabbitMQService);
  const userController = new UserController(createUserUseCase);

  // Setup Worker
  startWorker(rabbitMQService, userRepository);

  // Setup Routes
  const router = express.Router();
  userRoutes(router, userController);
  app.use('/api', router);

  return app;
};
