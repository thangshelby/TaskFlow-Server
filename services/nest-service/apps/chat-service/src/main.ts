import { Logger } from '@nestjs/common';
import { ReflectionService } from '@grpc/reflection';
import { NestFactory } from '@nestjs/core';
import { MicroserviceOptions, Transport } from '@nestjs/microservices';
import { join } from 'path';
import { GlobalHandleErrorInterceptor, GrpcAuthInterceptor } from '@nest-service/core';
import { ChatServiceModule } from './chat-service.module';

async function bootstrap() {
  const app = await NestFactory.createMicroservice<MicroserviceOptions>(ChatServiceModule, {
    transport: Transport.GRPC,
    options: {
      package: 'chat_service',
      url: '0.0.0.0:5003',
      protoPath: join(__dirname, 'protos/chat.proto'),
      loader: {
        includeDirs: [join(__dirname, 'protos')],
      },
      onLoadPackageDefinition: (pkg, server) => {
        new ReflectionService(pkg).addToServer(server);
      },
    },
  });
  app.useGlobalInterceptors(new GrpcAuthInterceptor(), new GlobalHandleErrorInterceptor());

  await app.listen();
  Logger.log(`🚀 gRPC server running at http://localhost:5003`);
}

bootstrap();
