import { UserUseCase } from '../domain/usecases/user';
import { Request, Response } from 'express';

export class AuthController {
  constructor(private userUseCase: UserUseCase) {}
  async login(req: Request, res: Response): Promise<void> {
    try {
      res.status(200).json({
        status: 'success'
      });
    } catch (error) {
      // TODO: Handle error & log
      console.log(error);
      res.status(400).json({ error: error });
    }
  }
  async register(req: Request, res: Response): Promise<void> {
    try {
      res.status(200).json({
        status: 'success'
      });
    } catch (error) {
      console.log(error);
      res.status(400).json({ error: error });
    }
  }
}
