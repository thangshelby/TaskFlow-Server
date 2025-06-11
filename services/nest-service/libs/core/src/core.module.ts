import { Module } from '@nestjs/common';
import { LogService } from './log/log.service';
import { KafkaService } from './queue/kafka.service';
import { ConfigModule } from '@nestjs/config';
import { CassandraService } from './cassandra/cassandra.service';

@Module({
  controllers: [],
  imports: [ConfigModule],
  providers: [LogService, KafkaService, CassandraService],
  exports: [LogService, KafkaService, CassandraService],
})
export class CoreModule {}
