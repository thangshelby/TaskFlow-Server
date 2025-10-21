import { ElacticSearchService } from '../elasticsearch/elastiSearchService.js'
import ProjectMemberModel from '../models/project_member.js'

export const searchIssues = async (req, res) => {
  try {
    const { q, project_ids, assignee_ids, priorities, status, last_updated, userId } = req.body

    const projects = await ProjectMemberModel.find({})
    const userProjects = projects.filter(
      (project) => project.user_id === userId && (project_ids?.length ? project_ids.includes(project.project_id) : true)
    )
    const elacticSearchService = new ElacticSearchService()
    const results = []
    for (const project of userProjects) {
      const issues = await elacticSearchService.searchIssues(
        project.project_id,
        q,
        last_updated,
        assignee_ids,
        status,
        priorities
      )
      results.push(...issues)
    }
    res.status(200).json(results)
  } catch (error) {
    console.error(error)
    res.status(500).json({ message: error.message })
  }
}

export const syncIssues = async (req, res) => {
  try {
    const { projectId, userId } = req.body
    const elacticSearchService = new ElacticSearchService()
    await elacticSearchService.indexIssuesForProject(projectId, userId)
    res.status(200).json({ message: 'Issues synced successfully' })
  } catch (error) {
    console.error(error)
    res.status(500).json({ message: error.message })
  }
}
