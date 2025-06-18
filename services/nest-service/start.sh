#!/bin/bash

# Get service name from the first argument
SERVICE_NAME=$1

# Check if argument is provided
if [ -z "$SERVICE_NAME" ]; then
  echo "❌ Usage: ./start.sh <service-name>"
  exit 1
fi

# Copy .proto files into local folders
echo "📂 Copying proto files..."
rm -rf ./apps/$SERVICE_NAME/src/protos/
mkdir -p ./apps/$SERVICE_NAME/src/protos/

cp -r ../../protos/* ./apps/$SERVICE_NAME/src/protos/

# Generate proto types
echo "🔧 Generating proto types for $SERVICE_NAME..."
protoc \
  --plugin=protoc-gen-ts_proto=./node_modules/.bin/protoc-gen-ts_proto \
  --ts_proto_out=./apps/$SERVICE_NAME/src/types \
  --ts_proto_opt=nestJs=true,outputTypePrefix=true \
  --proto_path=../../protos \
  $(find ../../protos -name "*.proto")

protoc \
  --plugin=protoc-gen-ts_proto=./node_modules/.bin/protoc-gen-ts_proto \
  --ts_proto_out=./libs/core/src/types \
  --ts_proto_opt=nestJs=true,outputTypePrefix=true \
  --proto_path=../../protos \
   $(find ../../protos -name "*.proto")
   