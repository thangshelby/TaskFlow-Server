import { Router } from 'express'
import * as controller from '../controller/index.js'

const router = Router()

router.post('/sync_issues', controller.syncIssues)
router.post('/search_issues', controller.searchIssues)

export default router
