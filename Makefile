container-up:
	docker compose up -d
container-down:
	docker compose down
node-server:
	cd services/node-service && npm run dev
mysql:
	docker start mysql-container
main-server:
	cd services/main-service && dotnet watch
.PHONY: container-up container-down node-server