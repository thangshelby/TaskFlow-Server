import { CallHandler, ExecutionContext, Injectable, NestInterceptor } from '@nestjs/common';
import { Observable } from 'rxjs';
import { RpcException } from '@nestjs/microservices';
import { Status } from '@grpc/grpc-js/build/src/constants';
import { Metadata } from '@grpc/grpc-js';
import { jwtDecode } from 'jwt-decode';

@Injectable()
export class GrpcAuthInterceptor implements NestInterceptor {
  intercept(context: ExecutionContext, next: CallHandler): Observable<unknown> {
    const metadata = context.getArgByIndex(1) as Metadata;

    const rawCookie = metadata.get('cookie')?.[0];
    const cookieHeader = Buffer.isBuffer(rawCookie) ? rawCookie.toString() : rawCookie;
    const token = this.getTokenFromCookie(cookieHeader);
    if (token) {
      try {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const payload = jwtDecode(token) as any;
        metadata.add('userId', String(payload.userId));
        metadata.add('userRole', String(payload.role));
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } catch (error: any) {
        throw new RpcException({
          code: Status.UNAUTHENTICATED,
          message: `Token verification failed: ${error.message}`,
        });
      }
    }

    return next.handle();
  }

  private getTokenFromCookie(cookieHeader?: string): string | null {
    if (!cookieHeader) return null;

    const cookies = cookieHeader.split(';');
    for (const cookie of cookies) {
      const trimmed = cookie.trim();
      if (trimmed.startsWith('token=')) {
        return trimmed.substring('token='.length);
      }
    }
    return null;
  }
}
