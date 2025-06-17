media-service:
	cd cmd/media-service && go mod tidy && go run main.go
	
.PHONY: media-service

PROTO_DIR=../../protos
PROTO_FILES=$(shell find $(PROTO_DIR) -name "*.proto" -not -path "*/google/*")

protoc:
	protoc -I=../../protos \
	--go_out=./types --go_opt=paths=source_relative \
	--go-grpc_out=./types --go-grpc_opt=paths=source_relative \
	$(PROTO_FILES)
.PHONY: protoc
