import { Injectable, NestInterceptor, ExecutionContext, CallHandler, Logger } from '@nestjs/common';
import { Observable, catchError } from 'rxjs';
import { RpcException } from '@nestjs/microservices';
import { status } from '@grpc/grpc-js';

@Injectable()
export class GlobalHandleErrorInterceptor implements NestInterceptor {
  private readonly logger = new Logger(GlobalHandleErrorInterceptor.name);

  intercept(context: ExecutionContext, next: CallHandler): Observable<unknown> {
    return next.handle().pipe(
      catchError((error) => {
        const handler = context.getHandler().name;

        this.logger.error(`[gRPC Error] ${handler}: ${error.message || error}`);

        throw new RpcException({
          code: status.INTERNAL,
          message: error.message || 'Internal server error',
        });
      })
    );
  }
}
