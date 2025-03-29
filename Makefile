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
sync-gateway:
	deck gateway sync kong.yaml
update-gateway:
	deck gateway dump -o kong.yaml 
gen-protobuf:
	protoc -I ./services/main-service/Protos --include_imports --include_source_info \
		--descriptor_set_out=./proto.pb $(shell find ./services/main-service/Protos -name "*.proto")
gen-testtoken:
	cd services/node-service && node genjwt.js

.PHONY: container-up container-down node-server sync-gateway update-gateway gen-protobuf