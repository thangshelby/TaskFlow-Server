import client from './client.js'
import { lastUpdatedAtOptions } from '../config/index.js'
import IssueModel from '../models/issue.js'
export class ElacticSearchService {
  async indexIssue(issue, projectId) {
    try {
      const indexName = `project-${projectId}-issues`

      // Ép về plain object (nếu issue là Mongoose document)

      await client.index({
        index: indexName,
        id: issue._id.toString(),
        document: {
          id: issue._id.toString(),
          title: issue.title,
          project_id: issue.project_id,
          assignee_id: issue.assignee_id,
          reporter_id: issue.reporter_id,
          priority: issue.priority,
          type: issue.type,
          column_id: issue.column_id,
          story_point: issue.story_point,
          created_at: issue.created_at,
          updated_at: issue.updated_at,
          due_date_from: issue.due_date_from,
          due_date_to: issue.due_date_to,
          key: issue.key,
          team_id: issue.team_id,
          summary: issue.summary,
          description: issue.description,
          attachments: issue.attachments
        }
      })

      console.log(`✅ Indexed issue ${issue._id} to ${indexName}`)
    } catch (error) {
      console.error(`❌ Error indexing issue ${issue._id} for project ${projectId}:`, error)
      throw error
    }
  }

  async createIndex(projectId) {
    const indexName = `project-${projectId}-issues`

    const exists = await client.indices.exists({ index: indexName })

    if (!exists) {
      await client.indices.create({
        index: indexName,
        body: {
          settings: {
            analysis: {
              tokenizer: {
                edge_ngram_tokenizer: {
                  type: 'edge_ngram',
                  min_gram: 3,
                  max_gram: 15,
                  token_chars: ['letter', 'digit']
                }
              },
              analyzer: {
                autocomplete_index: {
                  tokenizer: 'edge_ngram_tokenizer',
                  filter: ['lowercase', 'stop']
                },
                autocomplete_search: {
                  tokenizer: 'standard',
                  filter: ['lowercase', 'stop']
                }
              }
            }
          },
          mappings: {
            properties: {
              summary: {
                type: 'text',
                analyzer: 'autocomplete_index',
                search_analyzer: 'autocomplete_search'
              },
              key: {
                type: 'text',
                analyzer: 'autocomplete_index',
                search_analyzer: 'autocomplete_search'
              },
              title: { type: 'text' },
              description: { type: 'text' },
              project_id: { type: 'keyword' },
              assignee_id: { type: 'keyword' },
              reporter_id: { type: 'keyword' },
              priority: { type: 'keyword' },
              type: { type: 'keyword' },
              column_id: { type: 'keyword' },
              story_point: { type: 'integer' },
              created_at: { type: 'date' },
              updated_at: { type: 'date' }
            }
          }
        }
      })

      console.log(`✅ Created index '${indexName}'`)
    } else {
      console.log(`ℹ️ Index '${indexName}' already exists`)
    }
  }
  async searchIssues(projectIdIndex, q, last_updated, assignee_ids, status, priorities) {
    const must = []

    // fulltext search
    if (q) {
      must.push({
        multi_match: {
          query: q,
          fields: ['summary', 'key'],
          type: 'bool_prefix',
          fuzziness: 'AUTO'
        }
      })
    }

    must.push({
      range: {
        updated_at: {
          gte: lastUpdatedAtOptions.find((option) => option.label === last_updated)?.value,
          lte: 'now'
        }
      }
    })

    // filter theo assignee_id
    if (assignee_ids?.length) {
      must.push({
        terms: {
          assignee_id: assignee_ids
        }
      })
    }

    if (status?.length) {
      must.push({
        terms: {
          column_id: status
        }
      })
    }

    // filter theo priority
    if (priorities?.length) {
      must.push({
        terms: {
          priority: priorities
        }
      })
    }

    const query = {
      bool: {
        must
      }
    }
    const index = `project-${projectIdIndex}-issues`
    const result = await client.search({
      index,
      query,
      size: 100
    })
    return result.hits.hits.map((hit) => hit._source)
  }
  async indexIssuesForProject(projectId, userId) {
    try {
      const issues = await IssueModel.find({ project_id: projectId })
      if (issues.length === 0) {
        console.log(`ℹ️ No issues found for project ${projectId}`)
        return
      }
      for (const issue of issues) {
        await this.indexIssue(issue, projectId)
      }
      console.log(`✅ Indexed ${issues.length} issues for project ${projectId}`)
    } catch (error) {
      console.error(`❌ Error indexing project ${projectId}:`, error)
      throw error
    }
  }
  async generateApiKey(opts) {
    const body = await client.security.createApiKey({
      body: {
        name: 'earthquake_app',
        role_descriptors: {
          earthquakes_example_writer: {
            cluster: ['monitor'],
            index: [
              {
                names: ['earthquakes'],
                privileges: ['create_index', 'write', 'read', 'manage']
              }
            ]
          }
        }
      }
    })
    return Buffer.from(`${body.id}:${body.api_key}`).toString('base64')
  }
}
