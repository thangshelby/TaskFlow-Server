# ─────────────────────────────────────────────────────────
#  TaskFlow Server — Makefile
# ─────────────────────────────────────────────────────────
#  Dev  : make dev           (uses docker-compose.override.yml automatically)
#  Prod : make prod
#  Help : make help
# ─────────────────────────────────────────────────────────

COMPOSE_DEV  = docker compose
COMPOSE_PROD = docker compose -f docker-compose.yml -f docker-compose.prod.yml

# ── Dev environment (default) ────────────────────────────

dev: ## Start all containers in dev mode
	$(COMPOSE_DEV) up -d

dev-build: ## Rebuild and start dev containers
	make gen-protobuf
	$(COMPOSE_DEV) up --build -d
	docker image prune -f

dev-down: ## Stop dev containers
	$(COMPOSE_DEV) down

dev-logs: ## Tail dev container logs
	$(COMPOSE_DEV) logs -f

# ── Prod environment ─────────────────────────────────────

prod: ## Start all containers in prod mode
	$(COMPOSE_PROD) up -d

prod-build: ## Rebuild and start prod containers
	make gen-protobuf
	$(COMPOSE_PROD) up --build -d
	docker image prune -f

prod-down: ## Stop prod containers
	$(COMPOSE_PROD) down

prod-logs: ## Tail prod container logs
	$(COMPOSE_PROD) logs -f

# ── Common Docker helpers ────────────────────────────────

logs: ## Tail logs (dev)
	$(COMPOSE_DEV) logs -f

ps: ## Show running containers
	$(COMPOSE_DEV) ps

clean: ## Stop containers and remove volumes
	$(COMPOSE_DEV) down -v
	docker image prune -f

restart: ## Restart all containers (dev)
	$(COMPOSE_DEV) restart

# ── Local services (without Docker) ─────────────────────

main-service: ## Run main gRPC service locally (dotnet watch)
	cd services/main-service && dotnet watch

noti-service: ## Run notification service locally
	cd services/nest-service && npm run start notification-service

nest-server: ## Run a nest-service app (usage: make nest-server service=notification-service)
	@if [ -z "$(service)" ]; then \
		echo "Usage: make nest-server service=<service-name>"; \
	else \
		cd services/nest-service && npm run serve $(service); \
	fi

# ── Protobuf generation ─────────────────────────────────

gen-protobuf: ## Generate protobuf descriptor (Linux/Mac)
	protoc -I ./protos --include_imports --include_source_info \
		--descriptor_set_out=./proto.pb $$(find ./protos -name "*.proto")

gen-protobuf-wd: ## Generate protobuf descriptor (Windows)
	powershell -Command "$$protos = Get-ChildItem -Recurse -Filter *.proto -Path './protos' | ForEach-Object { $$_.FullName | Resolve-Path -Relative }; protoc -I './protos' --include_imports --include_source_info --descriptor_set_out=./proto.pb $$protos"

gen-protobuf-notification-wd: ## Generate notification protobuf (Windows)
	cd services\nest-service\apps\notification-service && \
	protoc --plugin=protoc-gen-ts_proto=..\..\node_modules\.bin\protoc-gen-ts_proto.cmd --ts_proto_out=.\src\types --ts_proto_opt=nestJs=true --proto_path=.\src\proto .\src\proto\notification.proto && \
	protoc -I .\src\proto -I .\src\proto\google\api --include_imports --include_source_info --descriptor_set_out=..\..\..\..\notification.pb .\src\proto\notification.proto

# ── Utilities ────────────────────────────────────────────

gen-testtoken: ## Generate a test JWT token
	cd services/node-service && node genjwt.js

mysql: ## Start local MySQL container
	docker start mysql-container

# ── Help ─────────────────────────────────────────────────

help: ## Show this help message
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | \
		awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-25s\033[0m %s\n", $$1, $$2}'

.PHONY: dev dev-build dev-down dev-logs \
        prod prod-build prod-down prod-logs \
        logs ps clean restart \
        main-service noti-service nest-server \
        gen-protobuf gen-protobuf-wd gen-protobuf-notification-wd \
        gen-testtoken mysql help