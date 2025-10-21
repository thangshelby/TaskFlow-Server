import { Kafka } from 'kafkajs'

import { ElacticSearchService } from '../elasticsearch/elastiSearchService.js'

export class ActivitiesConsumer {
  constructor({ kafkaHost = 'localhost:9092', topic = 'activities', groupId = 'activities-group' } = {}) {
    this.kafka = new Kafka({
      clientId: 'express-service',
      brokers: [kafkaHost]
    })
    this.consumer = this.kafka.consumer({ groupId })
    this.topic = topic
    this.running = false
    this.elacticSearchService = new ElacticSearchService()
  }

  async start() {
    console.log('📡 ActivitiesConsumer is listening...')

    await this.consumer.connect()
    await this.consumer.subscribe({ topic: this.topic, fromBeginning: true })

    this.running = true
    await this.consumer.run({
      eachMessage: async ({ topic, partition, message }) => {
        try {
          if (!message.value) {
            console.warn('⚠️ Received null message')
            return
          }

          const msgStr = message.value.toString()

          const parsed = JSON.parse(msgStr)

          await this.handleActivitiesMessage(this.elacticSearchService, parsed)
        } catch (err) {
          console.error('❌ Failed to process Kafka message:', err)
        }
      }
    })
  }

  async handleActivitiesMessage(elacticSearchService, message) {
    const action = message?.eventType
    switch (action) {
      case 'ACTIVITIES_ISSUE_CREATED':
        await elacticSearchService.indexIssue(message.data.newIssue, message.data.newIssue.project_id)
        console.log('💬 Indexed issue: ', message.data.newIssue)
        break
      case 'ACTIVITIES_ISSUE_CHANGED':
        await elacticSearchService.indexIssue(message.data.newIssue, message.data.newIssue.project_id)
        console.log('💬 Updated issue: ', message.data.newIssue)
        break
      case 'ACTIVITIES_PROJECT_CREATED':
        await elacticSearchService.indexIssuesForProject(message.data.projectId, message.data.userId)
        console.log('💬 Indexed project: ', message.data.projectId)
        break
      case 'ACTIVITIES_MEMBER_JOINED_PROJECT':
        await elacticSearchService.indexIssuesForProject(message.data.projectId, message.data.userId)
        console.log('💬 Indexed project: ', message.data.projectId)
        break
      default:
        console.log(`ℹ️ Unknown eventType: ${action}`)
        break
    }
  }

  async stop() {
    if (this.running) {
      await this.consumer.disconnect()
      this.running = false
      console.log('🛑 Kafka consumer stopped')
    }
  }
}
