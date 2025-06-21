import { ConfigModule, ConfigService } from '@nestjs/config';
import { Module } from '@nestjs/common';
import { LogService } from './log/log.service';
import { KafkaService } from './queue/kafka.service';
import { CassandraService } from './cassandra/cassandra.service';
import { MongooseModule } from '@nestjs/mongoose';
import { UserClientService } from './client/user-client.service';
import { ClientsModule, Transport } from '@nestjs/microservices';
import { join } from 'path';

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
            url: configService.get<string>('MAIN_SERVICE'), // use from config
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
  providers: [LogService, KafkaService, CassandraService, UserClientService],
  exports: [LogService, KafkaService, CassandraService, UserClientService],
})
export class CoreModule {}
