import { USER_EVENT, USER_QUEUE } from '~/domain/constant/user';
import { RabbitMQService } from '~/infras/queue/rabbitmq';
// import { ElasticsearchService } from '~/infras/repositories/elastic';
import { MySQLUserRepository } from '~/infras/repositories/mysql/user/user';

export async function startWorker(
  rabbitMQService: RabbitMQService,
  // elasticService: ElasticsearchService,
  userRepository: MySQLUserRepository
) {
  try {
    if (!rabbitMQService['channel']) {
      throw new Error('RabbitMQ channel is not initialized');
    }

    const channel = rabbitMQService['channel']; // Use existing channel
    await channel.assertQueue(USER_QUEUE, { durable: true });

    console.log(`🚀 Worker is listening for messages on queue: ${USER_QUEUE}`);

    channel.consume(
      USER_QUEUE,
      async (msg) => {
        if (msg !== null) {
          const data = JSON.parse(msg.content.toString());
          console.log(`📥 Processing event: ${data.event} - UserID: ${data.userId}`);

          if (data.event === USER_EVENT.CREATE_USER) {
            const user = await userRepository.findById(data.userId);
            if (user) {
              // TODO: Update elastic db sync
              console.log(`✅ User ${data.userId} processed successfully`);
            }
          }

          channel.ack(msg);
        }
      },
      { noAck: false }
    );
  } catch (error) {
    console.error('❌ Worker Error:', error);
  }
}
