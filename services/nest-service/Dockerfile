# Multi-stage build for NestJS notification service
FROM node:18-alpine AS builder

WORKDIR /app

# pnpm via corepack — avoids a full global npm install of pnpm
RUN corepack enable && corepack prepare pnpm@9.15.4 --activate

COPY package.json pnpm-lock.yaml ./

RUN pnpm install --frozen-lockfile

COPY . .

RUN pnpm run build \
  && pnpm prune --prod

# Production: only runtime files — no pnpm, no devDependencies, no second install
FROM node:18-alpine AS production

WORKDIR /app

ENV NODE_ENV=production

RUN addgroup -g 1001 -S nodejs \
  && adduser -S nestjs -u 1001

COPY --chown=nestjs:nodejs --from=builder /app/node_modules ./node_modules
COPY --chown=nestjs:nodejs --from=builder /app/dist ./dist

USER nestjs

EXPOSE 5002

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD node -e "require('http').get('http://localhost:5002/health', (res) => { process.exit(res.statusCode === 200 ? 0 : 1) })"

CMD ["node", "dist/apps/notification-service/main.js"]
