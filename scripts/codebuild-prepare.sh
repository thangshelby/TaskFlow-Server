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

# #region agent log
# Local Cursor path may not exist on CodeBuild; fall back to /tmp.
_DEFAULT_DBG="/home/ngothang/github/Task/.cursor/debug-fade0b.log"
if [ -n "${TASKFLOW_DEBUG_LOG:-}" ]; then
  _DBG_LOG="$TASKFLOW_DEBUG_LOG"
elif [ -d "$(dirname "$_DEFAULT_DBG")" ]; then
  _DBG_LOG="$_DEFAULT_DBG"
else
  _DBG_LOG="/tmp/taskflow-debug-fade0b.ndjson"
fi
_dbg() {
  local hyp="$1" loc="$2" msg="$3" data="$4"
  local ts; ts=$(date +%s%3N 2>/dev/null || echo 0)
  echo "[DBG:${hyp}] ${msg} >> ${data}" >&2
  mkdir -p "$(dirname "$_DBG_LOG")" 2>/dev/null || true
  printf '{"sessionId":"fade0b","timestamp":%s,"location":"%s","message":"%s","data":%s,"hypothesisId":"%s"}\n' \
    "$ts" "$loc" "$msg" "$data" "$hyp" >> "$_DBG_LOG" 2>/dev/null || true
}
_git_root=$(git rev-parse --show-toplevel 2>/dev/null || echo "NO_GIT")
_dbg "H-E" "prepare.sh:11" "env_context" \
  "{\"cwd\":\"$(pwd)\",\"git_root\":\"${_git_root}\",\"script_root\":\"${_root}\"}"
# #endregion agent log

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

# ── Always log git context ───────────────────────────────────────────────────
echo "[taskflow] ========== git context ==========" >&2
echo "[taskflow] CODEBUILD_RESOLVED_SOURCE_VERSION : ${CODEBUILD_RESOLVED_SOURCE_VERSION:-n/a}" >&2
echo "[taskflow] CODEBUILD_WEBHOOK_PREV_COMMIT     : ${CODEBUILD_WEBHOOK_PREV_COMMIT:-n/a}" >&2
echo "[taskflow] CODEBUILD_SERVICES                : ${CODEBUILD_SERVICES:-n/a}" >&2
echo "[taskflow] FORCE_FULL_BUILD                  : ${FORCE_FULL_BUILD:-0}" >&2
if [ -d .git ]; then
  echo "[taskflow] git log (last 3 commits):" >&2
  git log --oneline -3 2>&1 | sed 's/^/[taskflow]   /' >&2
  echo "[taskflow] git diff --name-only HEAD~1 HEAD:" >&2
  git diff --name-only HEAD~1 HEAD 2>/dev/null | sed 's/^/[taskflow]   /' >&2 || echo "[taskflow]   (no HEAD~1)" >&2
else
  echo "[taskflow] .git directory: NOT FOUND (ZIP source — no history)" >&2
fi
echo "[taskflow] ====================================" >&2

# ── Decide which services to build ──────────────────────────────────────────
BUILD_ENVOY=false
BUILD_MAIN=false
BUILD_NOTI=false

if [ "${FORCE_FULL_BUILD:-0}" = "1" ] || [ "${CODEBUILD_SERVICES:-}" = "all" ]; then
  echo "[taskflow] Mode: FORCE_FULL_BUILD / all" >&2
  BUILD_ENVOY=true
  BUILD_MAIN=true
  BUILD_NOTI=true

elif [ -n "${CODEBUILD_SERVICES:-}" ]; then
  echo "[taskflow] Mode: CODEBUILD_SERVICES=${CODEBUILD_SERVICES}" >&2
  IFS=',' read -ra PARTS <<< "$CODEBUILD_SERVICES"
  for raw in "${PARTS[@]}"; do
    s=$(echo "$raw" | xargs)
    [ -z "$s" ] && continue
    case "$s" in
      envoy)        BUILD_ENVOY=true ;;
      main)         BUILD_MAIN=true ;;
      notification) BUILD_NOTI=true ;;
      *)
        echo "[taskflow] ERROR: unknown CODEBUILD_SERVICES entry '$s' (valid: envoy, main, notification, all)" >&2
        exit 1
        ;;
    esac
  done

elif [ ! -d .git ]; then
  echo "[taskflow] Mode: ZIP fallback (no .git) — building all services" >&2
  echo "[taskflow] FIX: set 'Full clone' in CodePipeline Source action, or set CODEBUILD_SERVICES=<service>" >&2
  BUILD_ENVOY=true
  BUILD_MAIN=true
  BUILD_NOTI=true

else
  echo "[taskflow] Mode: git diff" >&2
  _base_br="${TASKFLOW_DIFF_BASE_BRANCH:-develop}"
  DIFF_FILES=""

  _prev="${CODEBUILD_WEBHOOK_PREV_COMMIT:-}"
  # #region agent log
  _dbg "H-A" "prepare.sh:97" "diff_branch_decision" \
    "{\"_prev\":\"${_prev}\",\"prev_nonempty\":$( [ -n "$_prev" ] && echo true || echo false )}"
  # #endregion agent log
  if [ -n "$_prev" ] && git rev-parse "$_prev" >/dev/null 2>&1; then
    echo "[taskflow] Diff: ${_prev}..HEAD  (CODEBUILD_WEBHOOK_PREV_COMMIT)" >&2
    DIFF_FILES=$(git diff --name-only "${_prev}" HEAD || true)
    # #region agent log
    _dbg "H-A,H-B" "prepare.sh:102" "diff_via_prev_commit" \
      "{\"DIFF_FILES_len\":$(printf '%s' "${DIFF_FILES}" | wc -c),\"DIFF_FILES\":\"$(printf '%s' "${DIFF_FILES}" | tr '\n' '|')\"}"
    # #endregion agent log
  elif git rev-parse HEAD~1 >/dev/null 2>&1; then
    echo "[taskflow] Diff: HEAD~1..HEAD" >&2
    DIFF_FILES=$(git diff --name-only HEAD~1 HEAD || true)
    # #region agent log
    _dbg "H-A,H-B" "prepare.sh:108" "diff_via_HEAD~1" \
      "{\"DIFF_FILES_len\":$(printf '%s' "${DIFF_FILES}" | wc -c),\"DIFF_FILES\":\"$(printf '%s' "${DIFF_FILES}" | tr '\n' '|')\"}"
    # #endregion agent log
  else
    echo "[taskflow] No HEAD~1 — fetching origin/${_base_br}..." >&2
    # #region agent log
    _dbg "H-A" "prepare.sh:113" "no_HEAD~1_branch_taken" "{\"base_br\":\"${_base_br}\"}"
    # #endregion agent log
    git fetch origin "${_base_br}" --depth=100 2>/dev/null || true
    if git rev-parse "origin/${_base_br}" >/dev/null 2>&1; then
      echo "[taskflow] Diff: origin/${_base_br}...HEAD" >&2
      DIFF_FILES=$(git diff --name-only "origin/${_base_br}"...HEAD || true)
      # #region agent log
      _dbg "H-A,H-B" "prepare.sh:119" "diff_via_origin_develop" \
        "{\"DIFF_FILES_len\":$(printf '%s' "${DIFF_FILES}" | wc -c),\"DIFF_FILES\":\"$(printf '%s' "${DIFF_FILES}" | tr '\n' '|')\"}"
      # #endregion agent log
    else
      echo "[taskflow] Cannot diff — building all services" >&2
      BUILD_ENVOY=true; BUILD_MAIN=true; BUILD_NOTI=true
    fi
  fi

  # CodePipeline/CodeBuild often uses a shallow clone (no HEAD~1). When the pushed
  # commit is already the tip of origin/<branch>, origin/...HEAD is empty even though
  # HEAD introduced files — fall back to the tree of the current commit only.
  if [ -z "${DIFF_FILES}" ] && [ "${BUILD_ENVOY}" = "false" ] && [ "${BUILD_MAIN}" = "false" ] && [ "${BUILD_NOTI}" = "false" ]; then
    echo "[taskflow] Diff range empty — using files from HEAD (shallow / tip-of-branch)" >&2
    DIFF_FILES=$(git diff-tree --no-commit-id --name-only -r HEAD 2>/dev/null || true)
    # #region agent log
    _dbg "H-A,H-B" "prepare.sh:fallback_head" "diff_via_HEAD_tree" \
      "{\"DIFF_FILES_len\":$(printf '%s' "${DIFF_FILES}" | wc -c),\"DIFF_FILES\":\"$(printf '%s' "${DIFF_FILES}" | tr '\n' '|')\"}"
    # #endregion agent log
  fi

  if [ -n "${DIFF_FILES}" ]; then
    echo "[taskflow] Files changed:" >&2
    echo "${DIFF_FILES}" | sed 's/^/[taskflow]   /' >&2
    while IFS= read -r f; do
      [[ -z "$f" ]] && continue
      # #region agent log
      _dbg "H-C,H-D" "prepare.sh:130" "file_in_loop" \
        "{\"f\":\"${f}\",\"f_len\":$(printf '%s' "${f}" | wc -c),\"f_hex\":\"$(printf '%s' "${f}" | xxd -p 2>/dev/null | tr -d '\n' | head -c 40)\"}"
      # #endregion agent log
      case "$f" in
        protos/*|proto.pb)
          BUILD_ENVOY=true; BUILD_MAIN=true; BUILD_NOTI=true ;;
        envoy.yaml|dockerfile)
          BUILD_ENVOY=true ;;
        services/main-service/*)
          BUILD_MAIN=true ;;
        services/nest-service/*)
          BUILD_NOTI=true ;;
        scripts/*|buildspec.yml)
          BUILD_ENVOY=true; BUILD_MAIN=true; BUILD_NOTI=true ;;
      esac
    done <<< "${DIFF_FILES}"
  else
    echo "[taskflow] No changed files matched any service path — nothing to build" >&2
  fi
fi

# ── Write env for subsequent phases ─────────────────────────────────────────
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

echo "[taskflow] BUILD_ENVOY=${BUILD_ENVOY}  BUILD_MAIN=${BUILD_MAIN}  BUILD_NOTI=${BUILD_NOTI}" >&2

if [ "${BUILD_ENVOY}" != "true" ] && [ "${BUILD_MAIN}" != "true" ] && [ "${BUILD_NOTI}" != "true" ]; then
  echo "[taskflow] Nothing to build. Set FORCE_FULL_BUILD=1 to override." >&2
fi

aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$ECR_REGISTRY"