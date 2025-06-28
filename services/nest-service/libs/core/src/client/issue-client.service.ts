import { IssueRes } from '@nest-service/core/types/base';
import { IssueServiceClient } from '@nest-service/core/types/main_service/issue';
import { Inject, Injectable, OnModuleInit } from '@nestjs/common';
import { ClientGrpc } from '@nestjs/microservices';
import { firstValueFrom } from 'rxjs';

@Injectable()
export class IssueClientService implements OnModuleInit {
  private issueGrpcService: IssueServiceClient;

  constructor(@Inject('ISSUE_PACKAGE') private client: ClientGrpc) {}

  onModuleInit() {
    this.issueGrpcService = this.client.getService<IssueServiceClient>('IssueService');
  }

  async getListIssues(issueIds: string[]): Promise<IssueRes[] | undefined> {
    const res = await firstValueFrom(this.issueGrpcService.listIssues({ page: 1, limit: 100, assigneeIds: [], columnIds: [], issueIds: issueIds, sprintIds: [], projectId: '' }));
    return res.data;
  }
}
