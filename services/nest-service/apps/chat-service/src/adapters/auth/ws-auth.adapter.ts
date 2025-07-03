import { Metadata } from '@grpc/grpc-js';
import { Injectable } from '@nestjs/common';
import { parse } from 'cookie';
import * as jwt from 'jsonwebtoken';

interface TokenPayload {
  userId: string;
  role: string;
}

@Injectable()
export class WsAuthAdapter {
  createMetadataFromCookie(cookieHeader?: string): Metadata {
    const metadata = new Metadata();

    if (!cookieHeader) {
      return metadata;
    }

    try {
      const cookies = parse(cookieHeader);
      const token = cookies.token;

      if (!token) {
        return metadata;
      }

      // Decode token without verification since gRPC will verify it
      const decoded = jwt.decode(token) as TokenPayload;

      if (decoded?.userId) {
        metadata.set('userId', decoded.userId);
      }
      if (decoded?.role) {
        metadata.set('userRole', decoded.role);
      }
    } catch (err) {
      console.error('Error parsing auth cookie:', err);
    }

    return metadata;
  }
}
