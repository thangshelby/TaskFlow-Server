import { createClient } from 'redis';

export class RedisService {
  private client;

  constructor() {
    this.client = createClient();
  }
  async connect() {
    try {
      await this.client.connect();
      console.log('✅ Redis Connected');
    } catch (error) {
      console.error('❌ Redis Connection Failed:', error);
      throw error;
    }
  }
  async get(key: string) {
    const data = await this.client.get(key);
    return data ? JSON.parse(data) : null;
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  async set(key: string, value: any, expiration: number = 60) {
    await this.client.set(key, JSON.stringify(value), { EX: expiration });
  }
}
