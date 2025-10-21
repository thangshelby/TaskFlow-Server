import express from 'express/index.js'
import cors from 'cors'
import { connectDatabase } from './config/connectDB.js'
import dotenv from 'dotenv'
import router from './router/index.js'
import { ActivitiesConsumer } from './kafka/activitiesConsumer.js'

dotenv.config()
const app = express()
app.use(express.urlencoded({ extended: true }))
app.use(express.json())
app.use(
  cors({
    origin: ['http://localhost:5173', 'http://localhost:3000'],
    credentials: true,
    methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
    allowedHeaders: ['Content-Type', 'Authorization'],
    optionsSuccessStatus: 200
  })
)
app.use(router)
export const db = connectDatabase()

const activitiesConsumer = new ActivitiesConsumer()

activitiesConsumer
  .start()
  .then(() => {
    console.log('✅ ActivitiesConsumer started')
  })
  .catch((error) => {
    console.error('❌ Error starting ActivitiesConsumer:', error)
  })

process.on('SIGINT', async () => {
  await activitiesConsumer.stop()
  process.exit(0)
})

const PORT = process.env.PORT || 3001
app.listen(PORT, () => console.log(`✅ Server started on port ${PORT}`))
