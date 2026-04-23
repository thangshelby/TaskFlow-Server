#!/bin/bash

# 1. Lấy tên service từ biến truyền vào
SERVICE_NAME=$1

if [ -z "$SERVICE_NAME" ]; then
  echo "❌ Usage: ./start.sh <service-name>"
  exit 1
fi

# 2. Copy .proto files
echo "📂 Copying proto files for $SERVICE_NAME..."
rm -rf ./apps/$SERVICE_NAME/src/protos/
mkdir -p ./apps/$SERVICE_NAME/src/protos/
cp -r ../../protos/* ./apps/$SERVICE_NAME/src/protos/

# 3. Generate proto types
echo "🔧 Generating proto types..."
rm -rf ./libs/core/src/types
mkdir -p ./libs/core/src/types

protoc \
  --plugin=protoc-gen-ts_proto=./node_modules/.bin/protoc-gen-ts_proto \
  --ts_proto_out=./libs/core/src/types \
  --ts_proto_opt=nestJs=true,outputTypePrefix=true \
  --proto_path=../../protos \
   $(find ../../protos -name "*.proto")

# 4. Kiểm tra xem gen type có lỗi không trước khi chạy service
if [ $? -eq 0 ]; then
  echo "✅ Proto types generated successfully."
  echo "🚀 Starting service: $SERVICE_NAME..."
  
  # Chạy service bằng Nest CLI (chế độ development)
  # Hoặc dùng: npx nest start $SERVICE_NAME --watch nếu muốn hot-reload
  npx nest start "$SERVICE_NAME"
else
  echo "❌ Failed to generate proto types."
  exit 1
fi