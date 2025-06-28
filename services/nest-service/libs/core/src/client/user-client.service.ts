import { Metadata } from '@grpc/grpc-js';
import { UserRes, UserServiceClient } from '@nest-service/core/types/main_service/user';
import { Inject, Injectable, OnModuleInit } from '@nestjs/common';
import { ClientGrpc } from '@nestjs/microservices';
import { firstValueFrom } from 'rxjs';

@Injectable()
export class UserClientService implements OnModuleInit {
  private userGrpcService: UserServiceClient;

  constructor(@Inject('USER_PACKAGE') private client: ClientGrpc) {}

  onModuleInit() {
    this.userGrpcService = this.client.getService<UserServiceClient>('UserService');
  }

  async getUserById(data: { userId?: string; metadata?: Metadata }): Promise<UserRes | undefined> {
    let user_id = data?.userId || '';

    if (data.metadata) {
      user_id = this.extractUserMetadata(data.metadata).userId;
    }

    const res = await firstValueFrom(this.userGrpcService.getById({ userId: user_id }));

    return res.data;
  }

  async getListUsers(userIds: string[]): Promise<UserRes[] | undefined> {
    const res = await firstValueFrom(this.userGrpcService.listUsers({ userIds: userIds, limit: 1000, page: 1 }));
    return res.data;
  }

  extractUserMetadata(metadata: Metadata): {
    userId: string;
    userRole: string;
  } {
    return {
      userId: (metadata.get('userId')?.[0] as string) || '',
      userRole: (metadata.get('userRole')?.[0] as string) || '',
    };
  }
}
