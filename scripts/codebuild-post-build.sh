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

if ! command -v jq >/dev/null 2>&1; then
  echo "jq is required for imagedefinitions.json" >&2
  exit 1
fi

json='[]'

push_and_record() {
  local name=$1
  local repo=$2
  docker push "${ECR_REGISTRY}/${repo}:${IMAGE_TAG}"
  docker push "${ECR_REGISTRY}/${repo}:latest"
  json=$(jq -n --argjson j "$json" --arg n "$name" --arg u "${ECR_REGISTRY}/${repo}:${IMAGE_TAG}" '$j + [{"name":$n,"imageUri":$u}]')
}

if [ "${BUILD_ENVOY}" = "true" ]; then
  push_and_record envoy "${ECR_REPO_ENVOY}"
fi
if [ "${BUILD_MAIN}" = "true" ]; then
  push_and_record main-service "${ECR_REPO_MAIN}"
fi
if [ "${BUILD_NOTI}" = "true" ]; then
  push_and_record notification-service "${ECR_REPO_NOTIFICATION}"
fi

echo "$json" | tee imagedefinitions.json
