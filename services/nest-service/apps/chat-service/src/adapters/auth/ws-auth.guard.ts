import { CanActivate, ExecutionContext, Injectable } from '@nestjs/common';
import { WsException } from '@nestjs/websockets';
import { JwtService } from '@nestjs/jwt';
import { AuthenticatedSocket } from '../websocket/chat.events';

interface JwtPayload {
  sub: string;
  [key: string]: any;
}

@Injectable()
export class WsAuthGuard implements CanActivate {
  constructor(private readonly jwtService: JwtService) {}

  async canActivate(context: ExecutionContext): Promise<boolean> {
    try {
      const client: AuthenticatedSocket = context.switchToWs().getClient();
      const cookies = client.handshake?.headers?.cookie;

      if (!cookies) {
        throw new WsException('No cookies found');
      }

      // Parse cookies string to find token
      const token = this.parseCookies(cookies)['token'];

      if (!token) {
        throw new WsException('Authentication token not found');
      }

      const payload = await this.jwtService.verifyAsync<JwtPayload>(token).catch(() => null);

      if (!payload?.sub) {
        throw new WsException('Invalid token payload');
      }

      // Store user data in socket for later use
      client.data = {
        userId: payload.sub,
        ...payload,
      };

      return true;
    } catch (error) {
      if (error instanceof WsException) {
        throw error;
      }
      throw new WsException('Invalid authentication token');
    }
  }

  private parseCookies(cookieHeader: string): { [key: string]: string } {
    const cookies: { [key: string]: string } = {};
    cookieHeader.split(';').forEach((cookie) => {
      const parts = cookie.split('=');
      const name = parts[0].trim();
      const value = parts[1]?.trim();
      if (name && value) {
        cookies[name] = value;
      }
    });
    return cookies;
  }
}
