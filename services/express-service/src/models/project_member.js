import mongoose from 'mongoose'

const ProjectMemberSchema = new mongoose.Schema(
  {
    project_id: { type: String, required: true },
    user_id: { type: String, required: true },
    role: {
      type: Number,
      required: true
    },
    is_pending: {
      type: Boolean,
      default: false
    }
  },
  {
    timestamps: { createdAt: 'created_at', updatedAt: 'updated_at' },
    versionKey: false
  }
)
const ProjectMemberModel = mongoose.model('Project_Member', ProjectMemberSchema)
export default ProjectMemberModel
