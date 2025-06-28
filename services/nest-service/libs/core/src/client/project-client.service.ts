import { ProjectRes, ProjectServiceClient } from '@nest-service/core/types/main_service/project';
import { Inject, Injectable, OnModuleInit } from '@nestjs/common';
import { ClientGrpc } from '@nestjs/microservices';
import { firstValueFrom } from 'rxjs';

@Injectable()
export class ProjectClientService implements OnModuleInit {
  private projectGrpcService: ProjectServiceClient;

  constructor(@Inject('PROJECT_PACKAGE') private client: ClientGrpc) {}

  onModuleInit() {
    this.projectGrpcService = this.client.getService<ProjectServiceClient>('ProjectService');
  }

  async getListProjects(projectIds: string[]): Promise<ProjectRes[] | undefined> {
    const res = await firstValueFrom(this.projectGrpcService.listProjects({ projectIds: projectIds, limit: 100, page: 1, kw: '', sort: '' }));
    return res.data;
  }
}
