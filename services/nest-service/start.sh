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
rm -rf protos
mkdir -p protos

cp -r ../../protos/* protos/

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
# Serve with Nx
echo "🚀 Serving $SERVICE_NAME with Nx..."
nx serve $SERVICE_NAME
