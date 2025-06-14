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
	@if [ -z "$(service)" ]; then \
		echo "Usage: make nest-server service=notification-service"; \
	else \
		cd services/nest-service && npm run serve $(service); \
	fi
mysql:
	docker start mysql-container
main-server:
	cd services/main-service && dotnet watch

gen-protobuf-main-service:
	protoc -I ./services/main-service/Protos --include_imports --include_source_info \
		--descriptor_set_out=./proto.pb $(shell find ./services/main-service/Protos -name "*.proto")

gen-protobuf-notification-service:
	cd services/nest-service/apps/notification-service && \
	protoc \
		--plugin=../../node_modules/.bin/protoc-gen-ts_proto \
		--ts_proto_out=./src/types \
		--ts_proto_opt=nestJs=true \
		--proto_path=./src/proto \
		./src/proto/notification.proto && \
	protoc \
		-I ./src/proto \
		-I ./src/proto/google/api \
		--include_imports \
		--include_source_info \
		--descriptor_set_out=../../../../notification.pb \
		./src/proto/notification.proto

gen-protobuf: 
	make gen-protobuf-main-service && make gen-protobuf-notification-service 

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

.PHONY: container-up container-down nest-server gen-protobuf sync cqlsh