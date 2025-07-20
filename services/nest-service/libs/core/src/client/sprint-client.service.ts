import { SprintRes, SprintServiceClient } from '@nest-service/core/types/main_service/sprint';
import { Inject, Injectable, OnModuleInit } from '@nestjs/common';
import { ClientGrpc } from '@nestjs/microservices';
import { firstValueFrom } from 'rxjs';
export interface GetListSprintsClientParams {
  sprintIds: string[];
  limit: number;
  page: number;
}
@Injectable()
export class SprintClientService implements OnModuleInit {
  private sprintGrpcService: SprintServiceClient;

  constructor(@Inject('SPRINT_PACKAGE') private client: ClientGrpc) {}

  onModuleInit() {
    this.sprintGrpcService = this.client.getService<SprintServiceClient>('SprintService');
  }

  async getListSprints(params: GetListSprintsClientParams): Promise<SprintRes[] | undefined> {
    const res = await firstValueFrom(this.sprintGrpcService.listSprints({ sprintIds: params.sprintIds, limit: params.limit, page: params.page }));
    return res.data;
  }
}
