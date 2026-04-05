  # Multi-stage build for NestJS notification service
  FROM node:18-alpine AS builder

  # Set working directory
  WORKDIR /app

  # Copy package files
  COPY package*.json pnpm-lock.yaml ./

  # Install pnpm
  RUN npm install -g pnpm

  # Install dependencies
  RUN pnpm install --frozen-lockfile

  # Copy source code
  COPY . .

  # Build the application
  RUN pnpm run build

# Production stage
FROM node:18-alpine AS production

# Set working directory
WORKDIR /app

# Copy package files
COPY package*.json pnpm-lock.yaml ./

# Install pnpm
RUN npm install -g pnpm

# Install only production dependencies
RUN pnpm install --frozen-lockfile --prod

# Copy built application from builder stage
COPY --from=builder /app/dist ./dist
COPY --from=builder /app/apps/notification-service/src/public ./dist/apps/notification-service/public
COPY --from=builder /app/apps/notification-service/src/protos ./dist/apps/notification-service/protos

# Create non-root user
RUN addgroup -g 1001 -S nodejs
RUN adduser -S nestjs -u 1001

# Change ownership of the app directory
RUN chown -R nestjs:nodejs /app
USER nestjs

# Expose port
EXPOSE 5002

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD node -e "require('http').get('http://localhost:5002/health', (res) => { process.exit(res.statusCode === 200 ? 0 : 1) })"

# Start the application
CMD ["node", "dist/apps/notification-service/main.js"]
