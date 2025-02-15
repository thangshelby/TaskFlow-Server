container-up:
	docker compose up -d
container-down:
	docker compose down
node-server:
	cd services/auth-services && npm run dev

.PHONY: container-up container-down node-server