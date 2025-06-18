import { Injectable, OnModuleInit, OnModuleDestroy } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { Client, QueryOptions } from 'cassandra-driver';
import { LogService } from '../log/log.service';

@Injectable()
export class CassandraService implements OnModuleInit, OnModuleDestroy {
  private client: Client;

  constructor(
    private readonly configService: ConfigService,
    private readonly logService: LogService,
  ) {
    const contactPoints = this.configService.get<string>('CASSANDRA_CONTACT_POINTS', 'localhost').split(',');
    const localDataCenter = this.configService.get<string>('CASSANDRA_LOCAL_DATA_CENTER', 'datacenter1');
    const keyspace = this.configService.get<string>('CASSANDRA_KEYSPACE', 'taskflow');

    this.client = new Client({
      contactPoints,
      localDataCenter,
      keyspace,
    });
  }

  async onModuleInit() {
    try {
      await this.client.connect();
      this.logService.log('Cassandra client connected');
    } catch (error: any) {
      this.logService.error('Failed to connect to Cassandra', error.stack);
      throw error;
    }
  }

  async onModuleDestroy() {
    try {
      await this.client.shutdown();
      this.logService.log('Cassandra client disconnected');
    } catch (error: any) {
      this.logService.error('Failed to disconnect from Cassandra', error.stack);
    }
  }

  getClient() {
    return this.client;
  }

  async executeQuery(query: string, params: any[], options?: QueryOptions): Promise<any> {
    try {
      const result = await this.client.execute(query, params, {
        prepare: true,
        ...options,
      });
      this.logService.log(`Executed query: ${query}`);
      return result;
    } catch (error: any) {
      this.logService.error(`Error executing Cassandra query: ${query}`, error.stack);
      throw error;
    }
  }
}
