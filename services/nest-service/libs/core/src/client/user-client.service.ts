import { Metadata } from '@grpc/grpc-js';
import { Injectable } from '@nestjs/common';

@Injectable()
export class UserClientService {
  async getUserById(data: { userId?: string; metadata?: Metadata }): Promise<any> {
    let user_id = '';

    if (data.metadata) {
      user_id = this.extractUserMetadata(data.metadata).userId;
    }
    // Example: GRPC call to main-service

    // Or mock it for now:
    return {
      id: user_id,
      name: 'John Doe',
      email: 'john@example.com',
    };
  }

  extractUserMetadata(metadata: Metadata): { userId: string; userRole: string } {
    return {
      userId: (metadata.get('userId')?.[0] as string) || '',
      userRole: (metadata.get('userRole')?.[0] as string) || '',
    };
  }
}
