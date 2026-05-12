#!/usr/bin/env bash
# Resolves which images to build (git diff, FORCE_FULL_BUILD, or CODEBUILD_SERVICES),
# writes /tmp/taskflow-build.env for later phases, then logs in to ECR.
set -euo pipefail

_here=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
_root=$(cd "$_here/.." && pwd)
cd "$_root"
if [ ! -f dockerfile ] && [ -d TaskFlow-Server ] && [ -f TaskFlow-Server/dockerfile ]; then
  cd TaskFlow-Server
fi

# shellcheck source=ecr-repos.defaults.sh
source "${_here}/ecr-repos.defaults.sh"

AWS_REGION="${AWS_REGION:-ap-southeast-1}"
AWS_ACCOUNT_ID="${AWS_ACCOUNT_ID:-017263836577}"
ECR_REGISTRY="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"

VER="${CODEBUILD_RESOLVED_SOURCE_VERSION:-}"
if [ -n "$VER" ]; then
  IMAGE_TAG=$(echo "$VER" | cut -c1-7)
else
  IMAGE_TAG="latest"
fi

BUILD_ENVOY=false
BUILD_MAIN=false
BUILD_NOTI=false

if [ "${FORCE_FULL_BUILD:-0}" = "1" ] || [ "${CODEBUILD_SERVICES:-}" = "all" ]; then
  BUILD_ENVOY=true
  BUILD_MAIN=true
  BUILD_NOTI=true
elif [ -n "${CODEBUILD_SERVICES:-}" ]; then
  IFS=',' read -ra PARTS <<< "$CODEBUILD_SERVICES"
  for raw in "${PARTS[@]}"; do
    s=$(echo "$raw" | xargs)
    [ -z "$s" ] && continue
    case "$s" in
      envoy) BUILD_ENVOY=true ;;
      main) BUILD_MAIN=true ;;
      notification) BUILD_NOTI=true ;;
      *)
        echo "Unknown CODEBUILD_SERVICES entry: $s (use envoy, main, notification)" >&2
        exit 1
        ;;
    esac
  done
else
  _base_br="${TASKFLOW_DIFF_BASE_BRANCH:-develop}"
  # Prefer HEAD~1 (full clone). CodePipeline/CodeBuild often uses depth=1 → no parent → fall back to origin/<branch>...HEAD.
  DIFF_FILES=""
  if git rev-parse HEAD~1 >/dev/null 2>&1; then
    echo "[taskflow] git diff HEAD~1..HEAD" >&2
    DIFF_FILES=$(git diff --name-only HEAD~1 HEAD || true)
  else
    echo "[taskflow] No HEAD~1 (shallow clone or single commit). Trying fetch origin/${_base_br}..." >&2
    git fetch origin "${_base_br}" --depth=100 2>/dev/null || true
    if git rev-parse "origin/${_base_br}" >/dev/null 2>&1; then
      echo "[taskflow] git diff origin/${_base_br}...HEAD" >&2
      DIFF_FILES=$(git diff --name-only "origin/${_base_br}"...HEAD || true)
    else
      echo "[taskflow] origin/${_base_br} not available after fetch; cannot diff." >&2
    fi
  fi

  if [ -z "${DIFF_FILES}" ] && ! git rev-parse HEAD~1 >/dev/null 2>&1 && ! git rev-parse "origin/${_base_br}" >/dev/null 2>&1; then
    echo "[taskflow] Fallback: building all services (set TASKFLOW_DIFF_BASE_BRANCH if your default branch is not develop)." >&2
    BUILD_ENVOY=true
    BUILD_MAIN=true
    BUILD_NOTI=true
  else
    while IFS= read -r f; do
      [[ -z "$f" ]] && continue
      if [[ "$f" == protos/* ]] || [[ "$f" == proto.pb ]]; then
        BUILD_ENVOY=true
        BUILD_MAIN=true
        BUILD_NOTI=true
      fi
      if [[ "$f" == envoy.yaml ]] || [[ "$f" == dockerfile ]]; then
        BUILD_ENVOY=true
      fi
      if [[ "$f" == services/main-service/* ]]; then
        BUILD_MAIN=true
      fi
      if [[ "$f" == services/nest-service/* ]]; then
        BUILD_NOTI=true
      fi
    done <<< "${DIFF_FILES}"
  fi
fi

{
  echo "export ECR_REGISTRY=\"${ECR_REGISTRY}\""
  echo "export IMAGE_TAG=\"${IMAGE_TAG}\""
  echo "export ECR_REPO_ENVOY=\"${ECR_REPO_ENVOY}\""
  echo "export ECR_REPO_MAIN=\"${ECR_REPO_MAIN}\""
  echo "export ECR_REPO_NOTIFICATION=\"${ECR_REPO_NOTIFICATION}\""
  echo "export BUILD_ENVOY=${BUILD_ENVOY}"
  echo "export BUILD_MAIN=${BUILD_MAIN}"
  echo "export BUILD_NOTI=${BUILD_NOTI}"
} > /tmp/taskflow-build.env

echo "[taskflow] BUILD_ENVOY=${BUILD_ENVOY} BUILD_MAIN=${BUILD_MAIN} BUILD_NOTI=${BUILD_NOTI}" >&2

if [ "${BUILD_ENVOY}" != "true" ] && [ "${BUILD_MAIN}" != "true" ] && [ "${BUILD_NOTI}" != "true" ]; then
  echo "[taskflow] No services selected from changed paths. Skipping docker builds. Set FORCE_FULL_BUILD=1 or CODEBUILD_SERVICES=envoy,main,notification."
fi

aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$ECR_REGISTRY"
