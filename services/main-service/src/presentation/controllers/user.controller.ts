import { Request, Response } from 'express';
import { UserUseCase } from '../../domain/usecases/user';
import { validatePaginationParams } from '~/utils/validator';

export class UserController {
  constructor(private userUseCase: UserUseCase) {}

  async createUser(req: Request, res: Response): Promise<void> {
    try {
      // TODO: handle validator params & body

      const { email, name, password } = req.body;

      const user = await this.userUseCase.createUser({
        email,
        name,
        password
      });
      res.status(201).json(user);
    } catch (error) {
      // TODO: Handle error & log
      console.log(error);
      res.status(400).json({ error: error });
    }
  }
  async getListUser(req: Request, res: Response): Promise<void> {
    try {
      res.status(200).json({
        status: 'success'
      });
      // const { page, limit } = validatePaginationParams(req);

      // const { data, total, totalPages } = await this.userUseCase.getUser({
      //   limit,
      //   page
      // });
      // res.status(200).json({
      //   // data: data,
      //   firstUser: data[0],
      //   status: 'success',
      //   pagination: {
      //     page,
      //     limit,
      //     total_pages: totalPages,
      //     total_items: total
      //   }
      // });
    } catch (error) {
      console.log(error);
      res.status(400).json({ error: error });
    }
  }
  async healthCheck(req: Request, res: Response): Promise<void> {
    try {
      res.status(200).json({
        status: 'Healthcheck ssuccess'
      });
    } catch (error) {
      console.log(error);
      res.status(400).json({ error: error });
    }
  }
  async updateUser(req: Request, res: Response): Promise<void> {
    try {
      const { email, password, name, id } = req.body;
      const updatedUser = await this.userUseCase.updateUser({
        id,
        email,
        password,
        name
      });

      res.status(200).json({
        status: 'success',
        data: updatedUser
      });
    } catch (error) {
      console.log(error);
      res.status(400).json({ error: error });
    }
  }
}
