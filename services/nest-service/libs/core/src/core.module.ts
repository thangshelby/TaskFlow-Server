import { ConfigModule, ConfigService } from '@nestjs/config';
import { Module } from '@nestjs/common';
import { LogService } from './log/log.service';
import { KafkaService } from './queue/kafka.service';
import { CassandraService } from './cassandra/cassandra.service';
import { MongooseModule } from '@nestjs/mongoose';
import { UserClientService } from './client/user-client.service';

@Module({
  controllers: [],
  imports: [
    ConfigModule,
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
