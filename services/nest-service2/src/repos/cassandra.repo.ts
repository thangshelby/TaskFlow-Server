import { Client, QueryOptions } from 'cassandra-driver';

export class CassandraBaseRepo {
  protected client: Client;

  constructor(keyspace: string) {
    this.client = new Client({
      contactPoints: ['localhost'],
      localDataCenter: 'datacenter1',
      keyspace: keyspace,
    });
  }
  getCasClient() {
    return this.client;
  }
  protected async executeQuery(query: string, params: any[], options?: QueryOptions): Promise<any> {
    try {
      const result = await this.client.execute(query, params, { prepare: true, ...options });
      return result;
    } catch (error) {
      console.error('Error executing Cassandra query:', error);
      throw error;
    }
  }
}
