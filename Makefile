container-up:
	docker compose up -d
container-down:
	docker compose down
node-server:
	cd services/main-service && npm run dev
mysql:
	docker start mysql-container

.PHONY: container-up container-down node-server