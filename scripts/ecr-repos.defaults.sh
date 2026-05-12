# ECR repository names — must match Private repositories in account (e.g. ap-southeast-1).
# Override via CodeBuild env if you ever rename a repo: ECR_REPO_ENVOY, ECR_REPO_MAIN, ECR_REPO_NOTIFICATION.
: "${ECR_REPO_ENVOY:=envoy-proxy}"
: "${ECR_REPO_MAIN:=main-service}"
: "${ECR_REPO_NOTIFICATION:=notification-service}"
