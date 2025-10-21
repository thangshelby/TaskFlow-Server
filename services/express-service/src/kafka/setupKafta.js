import { Kafka } from 'kafkajs'

const kafka = new Kafka({
  clientId: 'express-service',
  brokers: ['localhost:9092']
})

export { kafka }
