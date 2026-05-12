#!/usr/bin/env bash
# Builds Docker images according to flags in /tmp/taskflow-build.env (from codebuild-prepare.sh).
set -euo pipefail

_here=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
_root=$(cd "$_here/.." && pwd)
cd "$_root"
if [ ! -f dockerfile ] && [ -d TaskFlow-Server ] && [ -f TaskFlow-Server/dockerfile ]; then
  cd TaskFlow-Server
fi

source /tmp/taskflow-build.env

if [ "${BUILD_ENVOY}" = "true" ]; then
  docker build -t "${ECR_REPO_ENVOY}" -f dockerfile .
  docker tag "${ECR_REPO_ENVOY}:latest" "${ECR_REGISTRY}/${ECR_REPO_ENVOY}:${IMAGE_TAG}"
  docker tag "${ECR_REPO_ENVOY}:latest" "${ECR_REGISTRY}/${ECR_REPO_ENVOY}:latest"
fi

if [ "${BUILD_MAIN}" = "true" ]; then
  docker build -t "${ECR_REPO_MAIN}" -f services/main-service/Dockerfile .
  docker tag "${ECR_REPO_MAIN}:latest" "${ECR_REGISTRY}/${ECR_REPO_MAIN}:${IMAGE_TAG}"
  docker tag "${ECR_REPO_MAIN}:latest" "${ECR_REGISTRY}/${ECR_REPO_MAIN}:latest"
fi

if [ "${BUILD_NOTI}" = "true" ]; then
  docker build -t "${ECR_REPO_NOTIFICATION}" -f services/nest-service/Dockerfile services/nest-service
  docker tag "${ECR_REPO_NOTIFICATION}:latest" "${ECR_REGISTRY}/${ECR_REPO_NOTIFICATION}:${IMAGE_TAG}"
  docker tag "${ECR_REPO_NOTIFICATION}:latest" "${ECR_REGISTRY}/${ECR_REPO_NOTIFICATION}:latest"
fi
