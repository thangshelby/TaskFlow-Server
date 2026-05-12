import { ConfigModule, ConfigService } from '@nestjs/config';
import { Module } from '@nestjs/common';
import { LogService } from './log/log.service';
import { KafkaService } from './queue/kafka.service';
import { AwsService } from './queue/aws.service';
import { AwsQueueAdapter } from './queue/aws-queue.adapter';
import { KafkaQueueAdapter } from './queue/kafka-queue.adapter';
import { QUEUE_SERVICE_TOKEN } from './queue/queue.interface';
import { MongooseModule } from '@nestjs/mongoose';
import { UserClientService } from './client/user-client.service';
import { ClientsModule, Transport } from '@nestjs/microservices';
import { join } from 'path';
import { ProjectClientService } from '@nest-service/core/client/project-client.service';
import { SprintClientService } from '@nest-service/core/client/sprint-client.service';
import { IssueClientService } from '@nest-service/core/client/issue-client.service';

/** gRPC address for main-service (host:port). ECS must set MAIN_SERVICE to the reachable internal DNS name. */
const mainServiceGrpcUrl = (config: ConfigService) => config.get<string>('MAIN_SERVICE')?.trim() || 'main-service.TaskFlowNameSpace';

@Module({
  controllers: [],
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
    }),
    ClientsModule.registerAsync([
      {
        name: 'USER_PACKAGE',
        imports: [ConfigModule],
        inject: [ConfigService],
        useFactory: (configService: ConfigService) => ({
          transport: Transport.GRPC,
          options: {
            package: 'project_service',
            protoPath: join(__dirname, 'protos/main_service/user.proto'),
            loader: {
              includeDirs: [join(__dirname, 'protos')],
            },
            url: mainServiceGrpcUrl(configService),
          },
        }),
      },
      {
        name: 'PROJECT_PACKAGE',
        imports: [ConfigModule],
        inject: [ConfigService],
        useFactory: (configService: ConfigService) => ({
          transport: Transport.GRPC,
          options: {
            package: 'project_service',
            protoPath: join(__dirname, 'protos/main_service/project.proto'),
            loader: {
              includeDirs: [join(__dirname, 'protos')],
            },
            url: mainServiceGrpcUrl(configService),
          },
        }),
      },
      {
        name: 'SPRINT_PACKAGE',
        imports: [ConfigModule],
        inject: [ConfigService],
        useFactory: (configService: ConfigService) => ({
          transport: Transport.GRPC,
          options: {
            package: 'project_service',
            protoPath: join(__dirname, 'protos/main_service/sprint.proto'),
            loader: {
              includeDirs: [join(__dirname, 'protos')],
            },
            url: mainServiceGrpcUrl(configService),
          },
        }),
      },
      {
        name: 'ISSUE_PACKAGE',
        imports: [ConfigModule],
        inject: [ConfigService],
        useFactory: (configService: ConfigService) => ({
          transport: Transport.GRPC,
          options: {
            package: 'project_service',
            protoPath: join(__dirname, 'protos/main_service/issue.proto'),
            loader: {
              includeDirs: [join(__dirname, 'protos')],
            },
            url: mainServiceGrpcUrl(configService),
          },
        }),
      },
    ]),
    MongooseModule.forRootAsync({
      imports: [ConfigModule],
      inject: [ConfigService],
      useFactory: (configService: ConfigService) => ({
        uri: configService.get<string>('MONGODB_URI'),
        dbName: configService.get<string>('MONGODB_DB_NAME'),
      }),
    }),
  ],
  providers: [
    LogService,
    KafkaService,
    AwsService,
    KafkaQueueAdapter,
    AwsQueueAdapter,
    {
      provide: QUEUE_SERVICE_TOKEN,
      useFactory: (configService: ConfigService, kafkaQueueAdapter: KafkaQueueAdapter, awsQueueAdapter: AwsQueueAdapter) => {
        const provider = (configService.get<string>('QUEUE_PROVIDER') ?? 'aws').toLowerCase().trim();
        return provider === 'kafka' ? kafkaQueueAdapter : awsQueueAdapter;
      },
      inject: [ConfigService, KafkaQueueAdapter, AwsQueueAdapter],
    },
    UserClientService,
    ProjectClientService,
    SprintClientService,
    IssueClientService,
  ],
  exports: [LogService, KafkaService, AwsService, QUEUE_SERVICE_TOKEN, UserClientService, ProjectClientService, SprintClientService, IssueClientService],
})
export class CoreModule {}
