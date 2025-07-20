import { ProjectRes, ProjectServiceClient } from '@nest-service/core/types/main_service/project';
import { Inject, Injectable, OnModuleInit } from '@nestjs/common';
import { ClientGrpc } from '@nestjs/microservices';
import { firstValueFrom } from 'rxjs';
export interface GetListProjectsClientParams {
  projectIds: string[];
  limit: number;
  page: number;
}
@Injectable()
export class ProjectClientService implements OnModuleInit {
  private projectGrpcService: ProjectServiceClient;

  constructor(@Inject('PROJECT_PACKAGE') private client: ClientGrpc) {}

  onModuleInit() {
    this.projectGrpcService = this.client.getService<ProjectServiceClient>('ProjectService');
  }

  async getListProjects(param: GetListProjectsClientParams): Promise<ProjectRes[] | undefined> {
    const res = await firstValueFrom(this.projectGrpcService.listProjects({ projectIds: param.projectIds, limit: param.limit, page: param.page, kw: '', sort: '' }));
    return res.data;
  }
}
