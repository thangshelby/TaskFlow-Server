import { Client } from '@elastic/elasticsearch';
export class ElasticsearchService {
  private client: Client;

  constructor() {
    this.client = new Client({ node: 'http://localhost:9200' });
  }

  async init(): Promise<void> {
    try {
      const health = await this.client.cluster.health();
      console.log('✅ Elasticsearch Connected:', health.status);
    } catch (error) {
      console.error('❌ Elasticsearch Connection Failed:', error);
    }
  }

  async indexDocument(index: string, id: string, document: object): Promise<void> {
    try {
      await this.client.index({
        index,
        id,
        body: document,
        refresh: true
      });
      console.log(`✅ Document indexed in ${index} with ID: ${id}`);
    } catch (error) {
      console.error('❌ Elasticsearch Indexing Failed:', error);
    }
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  async search(index: string, query: object): Promise<any> {
    try {
      const result = await this.client.search({
        index,
        body: {
          query
        }
      });
      return result.hits.hits;
    } catch (error) {
      console.error('❌ Elasticsearch Search Failed:', error);
      return [];
    }
  }
}
