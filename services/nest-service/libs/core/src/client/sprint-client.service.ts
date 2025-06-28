import { SprintRes, SprintServiceClient } from '@nest-service/core/types/main_service/sprint';
import { Inject, Injectable, OnModuleInit } from '@nestjs/common';
import { ClientGrpc } from '@nestjs/microservices';
import { firstValueFrom } from 'rxjs';

@Injectable()
export class SprintClientService implements OnModuleInit {
  private sprintGrpcService: SprintServiceClient;

  constructor(@Inject('SPRINT_PACKAGE') private client: ClientGrpc) {}

  onModuleInit() {
    this.sprintGrpcService = this.client.getService<SprintServiceClient>('SprintService');
  }

  async getListSprints(sprintIds: string[]): Promise<SprintRes[] | undefined> {
    const res = await firstValueFrom(this.sprintGrpcService.listSprints({ sprintIds: sprintIds, limit: 100, page: 1 }));
    return res.data;
  }
}
