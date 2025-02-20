import { Router } from 'express';
import { UserController } from '~/presentation/controllers/user.controller';

export default (router: Router, userController: UserController): void => {
  router.get('/', (req, res) => userController.healthCheck(req, res));
  router.get('/users', (req, res) => userController.getListUser(req, res));
  router.post('/users', (req, res) => userController.createUser(req, res));
  router.put('/users', (req, res) => userController.updateUser(req, res));
};
