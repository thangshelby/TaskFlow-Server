import mongoose from 'mongoose'

const IssueSchema = new mongoose.Schema(
  {
    _id: { type: String }, // nếu bạn tự generate, không cần ObjectId
    title: { type: String, default: '' },
    project_id: { type: String, required: true },
    sprint_id: { type: String, default: '' },
    assignee_id: { type: String, default: '' },
    parent_id: { type: String, default: '' },
    reporter_id: { type: String, required: true },
    type: { type: String, default: 'Task' }, // Epic | Story | Task | Bug
    column_id: { type: String, default: '' },
    column: { type: String, default: null },
    priority: { type: String, default: 'Medium' },
    summary: { type: String, default: '' },
    description: { type: String, default: '' }, // nếu lưu JSON string thì để vậy
    story_point: { type: Number, default: 0 },
    attachments: { type: [String], default: [] },

    created_at: { type: Date, default: Date.now },
    completed_at: { type: Date, default: null },
    updated_at: { type: Date, default: Date.now },
    due_date_from: { type: Date, default: null },
    due_date_to: { type: Date, default: null },

    creator_id: { type: String, required: true },
    key: { type: String, required: true },
    team_id: { type: String, required: true }
  },
  {
    timestamps: { createdAt: 'created_at', updatedAt: 'updated_at' }
  }
)

const IssueModel = mongoose.model('Issue', IssueSchema)
export default IssueModel
