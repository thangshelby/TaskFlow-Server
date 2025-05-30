container-up:
	docker compose up -d
container-down:
	docker compose down
build:
	docker compose down
	docker compose up --build -d
	docker image prune -f 
logs:
	docker compose logs -f
nest-server:
	cd services/nest-service && pnpm run start:dev
mysql:
	docker start mysql-container
main-server:
	cd services/main-service && dotnet watch
sync-gateway:
	deck gateway sync kong.yaml
update-gateway:
	deck gateway dump -o kong.yaml 
gen-protobuf:
	protoc -I ./services/main-service/Protos --include_imports --include_source_info \
		--descriptor_set_out=./proto.pb $(shell find ./services/main-service/Protos -name "*.proto")
gen-protobuf-wd:
	powershell -Command "$$protos = Get-ChildItem -Recurse -Filter *.proto -Path './services/main-service/Protos' | ForEach-Object { $$_.FullName | Resolve-Path -Relative }; protoc -I './services/main-service/Protos' --include_imports --include_source_info --descriptor_set_out=./proto.pb $$protos"
gen-testtoken:
	cd services/node-service && node genjwt.js
sync:
	docker compose down 
	make gen-protobuf 
	docker compose up -d
sync-wd:
	docker compose down 
	make gen-protobuf-wd
	make build
cqlsh:
	docker exec -it cassandra cqlsh

.PHONY: container-up container-down nest-server sync-gateway update-gateway gen-protobuf sync cqlsh