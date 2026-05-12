#!/usr/bin/env bash
# Pushes images that were built and writes imagedefinitions.json for ECS/CodeDeploy.
set -euo pipefail

_here=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
_root=$(cd "$_here/.." && pwd)
cd "$_root"
if [ ! -f dockerfile ] && [ -d TaskFlow-Server ] && [ -f TaskFlow-Server/dockerfile ]; then
  cd TaskFlow-Server
fi

source /tmp/taskflow-build.env

# Write an empty array first so the artifact file always exists even if nothing is built.
echo '[]' > imagedefinitions.json

ENTRIES=""

push_image() {
  local repo=$1
  docker push "${ECR_REGISTRY}/${repo}:${IMAGE_TAG}"
  docker push "${ECR_REGISTRY}/${repo}:latest"
}

if [ "${BUILD_ENVOY}" = "true" ]; then
  push_image "${ECR_REPO_ENVOY}"
  ENTRIES="${ENTRIES}{\"name\":\"envoy\",\"imageUri\":\"${ECR_REGISTRY}/${ECR_REPO_ENVOY}:${IMAGE_TAG}\"},"
fi
if [ "${BUILD_MAIN}" = "true" ]; then
  push_image "${ECR_REPO_MAIN}"
  ENTRIES="${ENTRIES}{\"name\":\"main-service\",\"imageUri\":\"${ECR_REGISTRY}/${ECR_REPO_MAIN}:${IMAGE_TAG}\"},"
fi
if [ "${BUILD_NOTI}" = "true" ]; then
  push_image "${ECR_REPO_NOTIFICATION}"
  ENTRIES="${ENTRIES}{\"name\":\"notification-service\",\"imageUri\":\"${ECR_REGISTRY}/${ECR_REPO_NOTIFICATION}:${IMAGE_TAG}\"},"
fi

# Build final JSON array (strip trailing comma).
if [ -n "${ENTRIES}" ]; then
  ENTRIES="${ENTRIES%,}"
  printf '[%s]' "${ENTRIES}" > imagedefinitions.json
fi

echo "[taskflow] imagedefinitions.json → $(cat imagedefinitions.json)"
