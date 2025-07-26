// interceptors/http-auth.interceptor.ts
import { Injectable, NestInterceptor, ExecutionContext, CallHandler, UnauthorizedException } from '@nestjs/common';
import { Observable } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

@Injectable()
export class HttpAuthInterceptor implements NestInterceptor {
  intercept(context: ExecutionContext, next: CallHandler): Observable<any> {
    const request = context.switchToHttp().getRequest();

    const cookieHeader: string | undefined = request.headers.cookie;
    const token = this.getTokenFromCookie(cookieHeader);

    if (!token) {
      throw new UnauthorizedException('No token found in cookies');
    }

    try {
      const payload = jwtDecode(token) as any;
      request.user = {
        userId: payload.userId,
        role: payload.role,
      };
    } catch (error: any) {
      throw new UnauthorizedException(`Invalid token: ${error.message}`);
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
