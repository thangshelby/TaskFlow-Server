import { ConfigModule, ConfigService } from '@nestjs/config';
import { Module } from '@nestjs/common';
import { LogService } from './log/log.service';
import { KafkaService } from './queue/kafka.service';
import { CassandraService } from './cassandra/cassandra.service';
import { MongooseModule } from '@nestjs/mongoose';

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
  providers: [LogService, KafkaService, CassandraService],
  exports: [LogService, KafkaService, CassandraService],
})
export class CoreModule {}
