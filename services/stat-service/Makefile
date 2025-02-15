PROTO_DIR=proto
PROTO_FILES=$(shell find $(PROTO_DIR) -name "*.proto")

protoc:
	protoc -I=$(PROTO_DIR) --go_out=. --go-grpc_out=. $(PROTO_FILES)

.PHONY: protoc