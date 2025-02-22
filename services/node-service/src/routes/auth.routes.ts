import { Router } from 'express';
import { AuthController } from '~/controllers/auth.controller';

export default (router: Router, authController: AuthController): void => {
  router.post('/auth/login', (req, res) => authController.login(req, res));
  router.post('/auth/register', (req, res) => authController.register(req, res));
};
