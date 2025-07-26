// interceptors/transform-response.interceptor.ts
import { CallHandler, ExecutionContext, Injectable, NestInterceptor } from '@nestjs/common';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable()
export class TransformResponseInterceptor implements NestInterceptor {
  private toSnakeCase(str: string): string {
    return str.replace(/([A-Z])/g, '_$1').toLowerCase();
  }

  private convertToSnakeCase(obj: any): any {
    if (Array.isArray(obj)) {
      // eslint-disable-next-line @typescript-eslint/no-unsafe-return
      return obj.map((item) => this.convertToSnakeCase(item));
    }

    if (obj !== null && typeof obj === 'object') {
      return Object.entries(obj).reduce((acc: Record<string, any>, [key, value]) => {
        const newKey = this.toSnakeCase(key);
        acc[newKey] = this.convertToSnakeCase(value);
        return acc;
      }, {});
    }

    return obj;
  }

  intercept(context: ExecutionContext, next: CallHandler): Observable<any> {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-return
    return next.handle().pipe(map((data) => this.convertToSnakeCase(data)));
  }
}
