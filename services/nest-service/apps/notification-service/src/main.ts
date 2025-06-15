import { Logger } from '@nestjs/common';
import { ReflectionService } from '@grpc/reflection';
import { NestFactory } from '@nestjs/core';
import { MicroserviceOptions, Transport } from '@nestjs/microservices';
import { NotificationModule } from '@notification-service/notification.module';
import { join } from 'path';
import { GlobalHandleErrorInterceptor, GrpcAuthInterceptor } from '@nest-service/core';

async function bootstrap() {
  const app = await NestFactory.createMicroservice<MicroserviceOptions>(NotificationModule, {
    transport: Transport.GRPC,
    options: {
      package: 'notification_service',
      protoPath: join(__dirname, 'proto/notification.proto'),
      url: '0.0.0.0:5002',
      loader: {
        includeDirs: [join(__dirname, 'proto')],
      },
      onLoadPackageDefinition: (pkg, server) => {
        new ReflectionService(pkg).addToServer(server);
      },
    },
  });
  app.useGlobalInterceptors(new GrpcAuthInterceptor(), new GlobalHandleErrorInterceptor());

  await app.listen();
  Logger.log(`🚀 gRPC server running at http://localhost:5002`);
}

bootstrap();
