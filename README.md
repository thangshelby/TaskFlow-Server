# TaskFlow AWS Microservices DevSecOps Platform

![AWS EKS](https://img.shields.io/badge/AWS%20EKS-Kubernetes-FF9900?logo=amazoneks&logoColor=white)
![Terraform](https://img.shields.io/badge/Terraform-IaC-844FBA?logo=terraform&logoColor=white)
![Argo CD](https://img.shields.io/badge/Argo%20CD-GitOps-EF7B4D?logo=argo&logoColor=white)
![GitHub Actions](https://img.shields.io/badge/GitHub%20Actions-CI%2FCD-2088FF?logo=githubactions&logoColor=white)
![SonarQube](https://img.shields.io/badge/SonarQube-Code%20Quality-4E9BCD?logo=sonarqube&logoColor=white)
![Trivy](https://img.shields.io/badge/Trivy-Security%20Gate-1904DA?logo=aqua&logoColor=white)
![Prometheus](https://img.shields.io/badge/Prometheus-Monitoring-E6522C?logo=prometheus&logoColor=white)
![Grafana](https://img.shields.io/badge/Grafana-Dashboards-F46800?logo=grafana&logoColor=white)
![Alertmanager](https://img.shields.io/badge/Alertmanager-Alerts-E6522C?logo=prometheus&logoColor=white)
![Kyverno](https://img.shields.io/badge/Kyverno-Policy%20as%20Code-326CE5?logo=kubernetes&logoColor=white)
![Falco](https://img.shields.io/badge/Falco-Runtime%20Security-00AEC7)
![Nexus](https://img.shields.io/badge/Nexus-Artifacts-1B1C30?logo=sonatype&logoColor=white)
![Amazon ECR](https://img.shields.io/badge/Amazon%20ECR-Registry-FF9900?logo=amazonecr&logoColor=white)
![.NET](https://img.shields.io/badge/.NET%209-API-512BD4?logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-Vite-61DAFB?logo=react&logoColor=black)

Shields above sketch a full DevSecOps toolchain; **what this monorepo implements today** is primarily **AWS CloudFormation (VPC + EC2)**, **Docker Compose on EC2**, **Amazon ECR**, **GitHub Actions** for backend images and redeploy, **AWS CodeBuild** specs for **ECR** (backend) and **S3 + CloudFront** (frontend). **TaskFlow-Server** targets **.NET 10** (`Project Sdk="Microsoft.NET.Sdk.Web"` → `net10.0`), not .NET 9 as on the badge line.

This workspace combines the **TaskFlow** React/Vite SPA with **TaskFlow-Server**, a polyglot backend (ASP.NET Core **main-service**, NestJS **notification-service**, **Envoy** API gateway, Kafka, Redis, Elasticsearch/Kibana and related dependencies via Compose). The goal is not only to run the product but to document how builds reach AWS.

![TaskFlow platform overview](./image.png)

Placeholders: export `image.png` from [TaskFlow-Server/DIAGRAM.drawio](TaskFlow-Server/DIAGRAM.drawio) or add your own screenshot at the repo root.

## Table of Contents

- [Learning Objectives](#learning-objectives)
- [Architecture](#architecture)
- [Platform Layers](#platform-layers)
- [Folder Responsibility](#folder-responsibility)
- [Repository Structure](#repository-structure)
- [Current CI/CD Flow](#current-cicd-flow)
- [GitOps Runtime Flow](#gitops-runtime-flow)
- [Prerequisites](#prerequisites)
- [Run Locally](#run-locally)
- [Security Stack Setup](#security-stack-setup)
- [EC2 Build Host Setup](#ec2-build-host-setup)
- [GitHub Actions Secrets](#github-actions-secrets)
- [ECR, EKS, and Argo CD Setup](#ecr-eks-and-argo-cd-setup)
- [Pipeline Readiness Checklist](#pipeline-readiness-checklist)
- [Troubleshooting](#troubleshooting)
- [Documentation Index](#documentation-index)

## Learning Objectives

By walking through this repository, you should be able to explain and operate:

| Area | What this project demonstrates |
|---|---|
| Infrastructure as Code | Nested **AWS CloudFormation** stacks under `cloud-infrastructure/` for VPC subnets and an EC2 backend host with Docker bootstrap user data. |
| CI/CD | **GitHub Actions** (`TaskFlow-Server`) builds `main-service` and Envoy images, pushes **latest** to **ECR**, then **SSH** onto EC2 to `docker compose pull` / `up`. Optional **CodeBuild** (`buildspec.yml`) drives broader image pushes or SPA publish to **S3** / **CloudFront**. |
| GitOps | Not Argo CD here: deployment **desired state** is **Compose files + images** on EC2; Git pushes trigger CI only when workflows live at the Git repo root (see caveat below). |
| Kubernetes runtime | Not used in this repo; runtime orchestration is **Docker Compose** (`docker-compose.yml` + `docker-compose.prod.yml`). |
| Supply chain security | **ECR** as registry; workflow tags **`latest`** today (immutable digest tags can be adopted separately). Nexus/Trivy/Sonar artifacts are **not** wired in-tree. |
| Cluster security | EC2 security groups and IAM policies from CloudFormation; no Kyverno/Falco manifests in this workspace. |
| Observability | Envoy routing and dependency logs inside Compose; Prometheus/Grafana stacks are **not** configured here beyond badge references. |

## Architecture

```mermaid
flowchart TB
  dev[Developer] --> git[GitHub_Repositories]
  git --> gha_fe[GitHubActions_TaskFlow]
  git --> gha_be[GitHubActions_TaskFlow_Server]

  gha_fe --> build_fe[Vite_build]
  build_fe --> cb_fe[CodeBuild_optional]
  cb_fe --> s3[S3_bucket]

  gha_be --> docker_be[Docker_build_main_service]
  gha_be --> docker_env[Docker_build_envoy]
  docker_be --> ecr[Amazon_ECR]
  docker_env --> ecr

  cb_fe -. CloudFront_invalid .-> cf[CloudFront]

  cfn[CloudFormation_stacks] --> vpc[VPC_and_subnets]
  vpc --> ec2[EC2_backend_host]

  ecr --> ec2
  gha_be --> ssh[SSH_deploy_job]
  ssh --> compose[Docker_Compose_prod]

  compose --> envoy[Envoy_container]
  compose --> grpc[grpc_service_main_service]

  grpc --> deps[(Kafka_Redis_DB_elasticsearch)]

  sec_future[Sonar_Trivy_Kyverno_placeholder] -. not_in_repo .-> git
```

## Platform Layers

```mermaid
flowchart TB
  subgraph Source[Source_and_CI]
    code_fe[TaskFlow_SPA]
    code_be[TaskFlow_Server]
    gha_fe[GitHubActions_FE]
    gha_be[GitHubActions_Server]
    cb_be[CodeBuild_backend_optional]
    cb_fe[CodeBuild_frontend_optional]
    code_fe --> gha_fe
    code_be --> gha_be
    code_be --> cb_be
    code_fe --> cb_fe
  end

  subgraph Build[Image_and_asset_supply_chain]
    ecr[Amazon_ECR]
    s3[S3_Static_assets]
    gha_be --> ecr
    cb_be --> ecr
    cb_fe --> s3
  end

  subgraph Infra[AWS_Infrastructure]
    cfn[CloudFormation]
    vpc[VPC_and_subnets]
    ec2[EC2_instance]
    cfn --> vpc
    vpc --> ec2
  end

  subgraph Runtime[Compose_Runtime_on_EC2]
    app[Envoy_and_microservices]
    data[Kafka_Redis_search_optional_profiles]
  end

  ecr --> app
  ec2 --> app
  app --> data
```

High-level idea:

- GitHub Actions builds backend images when paths change under `TaskFlow-Server` and pushes them to **ECR**.
- The same workflow SSHs into **EC2**, logs into **ECR**, and rolls selected Compose services (**grpc-service**, **envoy**).
- **CloudFormation** provisions networking and the Docker-ready EC2 host.
- Frontend CI builds on **`develop`**; production SPA delivery can follow **`TaskFlow/buildspec.yml`** (S3 sync + CloudFront invalidation).

## Folder Responsibility

This workspace separates SPA source, backend services, IaC templates, and optional load tooling:

```text
TaskFlow/              = React/Vite SPA (pnpm), FE CI workflow, FE CodeBuild spec
TaskFlow-Server/       = Envoy + microservices + protos + Compose + BE CI/CD workflow + BE CodeBuild spec
cloud-infrastructure/  = CloudFormation root/nested stacks (+ optional ECS scheduler Lambda YAML)
TaskFlowTesting/       = k6 load tests (small harness)
```

Examples:

| Path | Role |
|---|---|
| `TaskFlow-Server/docker-compose.yml` | Local/dev Compose definitions and profiles (`grpc_service`, `notification_service`, `kafka`, `redis`, `elasticsearch`, …). |
| `TaskFlow-Server/docker-compose.prod.yml` | Production overrides: pull **`envoy-proxy`** and **`main-service`** images from **`${ECR_REGISTRY}`**. |
| `TaskFlow-Server/.github/workflows/deploy-main_service-ecr.yml` | Build/push **`main-service`** and **`envoy-proxy`** to ECR; redeploy on EC2 via SSH. |
| `TaskFlow/buildspec.yml` | Example CodeBuild pipeline: **`pnpm build`**, **`aws s3 sync`** to **`frontend.taskkfloww.shop`**, CloudFront invalidation **E2RGM9XABY19D9**. |
| `cloud-infrastructure/root.yaml` | Root stack composing networking + compute nested stacks. |

In short:

```text
TaskFlow/             = SPA client
TaskFlow-Server/      = API gateway + services + containers + backend CI/CD
cloud-infrastructure/ = VPC + EC2 (+ optional ECS scheduler templates)
```

## Repository Structure

```text
.
|-- TaskFlow/                       # React/Vite SPA (pnpm)
|-- TaskFlow-Server/                # Envoy, main-service (.NET), Nest notification-service, protos, Compose
|   |-- .github/workflows/        # Backend CI/CD (ECR + EC2 deploy)
|   |-- services/main-service/      # ASP.NET Core main-service Dockerfile build context root ..
|   |-- services/nest-service/      # NestJS notification-service
|   |-- protos/                     # gRPC/protobuf definitions shared with main-service
|   |-- envoy.yaml                  # Envoy routing config (wired into envoy image build)
|   |-- dockerfile                  # Envoy proxy image build at repo root
|   |-- docker-compose.yml          # Dev compose + profiles
|   |-- docker-compose.prod.yml     # Prod overrides (ECR images)
|   |-- buildspec.yml               # CodeBuild ECR workflow helper scripts
|   `-- DIAGRAM.drawio              # Architecture diagram source (export for ./image.png)
|-- cloud-infrastructure/           # CloudFormation templates (VPC, EC2, ALB/messaging stubs, ECS Lambda schedules)
|-- TaskFlowTesting/                # k6 tests
|-- TaskFlow/.github/workflows/     # Frontend Node CI on develop / PRs
`-- TaskFlow/buildspec.yml          # SPA CodeBuild → S3 / CloudFront (project-specific bucket & distribution IDs)
```

Key operational files:

| File | Purpose |
|---|---|
| `TaskFlow-Server/.github/workflows/deploy-main_service-ecr.yml` | Path-filtered workflow on **`develop`**: build/push **`main-service`** and **`envoy-proxy`** (`latest`), SSH deploy Compose services on EC2. |
| `TaskFlow/.github/workflows/node.js.yml` | **`pnpm install`** / **`pnpm run build`** on Node **22.x** and **24.x** for **`develop`** and PRs. |
| `TaskFlow-Server/services/main-service/Dockerfile` | Multi-stage build for **`main-service`** image (context is **`TaskFlow-Server`** root). |
| `TaskFlow-Server/dockerfile` | Envoy image built from **`envoy.yaml`**. |
| `TaskFlow-Server/buildspec.yml` | Documents optional CodeBuild-driven builds for **`envoy-proxy`**, **`main-service`**, **`notification-service`** (see comments for region/account defaults). |
| `cloud-infrastructure/root.yaml` | Orchestrates **`networking.yaml`** then **`compute.yaml`**. |

## Current CI/CD Flow

GitHub Actions for **TaskFlow-Server** runs on pushes to:

```text
develop
```

when changed paths match:

```text
services/main-service/**
protos/**
envoy.yaml
dockerfile
.github/workflows/deploy-main_service-ecr.yml
```

There is **no** skip phrase such as `ci: update image tag` in this workflow (nothing commits manifests back to Git).

Main flow (**TaskFlow-Server**):

1. **detect-changes** (`dorny/paths-filter`): flags **`main-service`** vs **envoy** paths.
2. **build-main-service** (if protos/main-service changed): configure AWS credentials, login **ECR**, `docker build` with **`services/main-service/Dockerfile`**, push **`$REGISTRY/main-service:latest`** (passes **`REDIS_CONNECTIONSTRING`** and optional AWS keys as build-args).
3. **build-envoy** (if `envoy.yaml` / root **`dockerfile`** changed): login **ECR**, build root **`dockerfile`**, push **`$REGISTRY/envoy-proxy:latest`**.
4. **deploy** (if either build succeeded): SSH to **`EC2_HOST`** as **`EC2_USERNAME`** with **`EC2_SSH_KEY`**, `docker login` to **ECR**, `cd ~/TaskFlow-Server`, **`docker compose -f docker-compose.yml -f docker-compose.prod.yml pull`** then **`up -d`** for **`grpc-service`** and/or **`envoy`** depending on which build succeeded.

Frontend (**TaskFlow**):

- **`node.js.yml`** runs **`pnpm install`** and **`pnpm run build`** on **`develop`** and PRs targeting **`develop`** (matrix Node **22.x** / **24.x**).

Optional **AWS CodeBuild**:

- **`TaskFlow-Server/buildspec.yml`** shells out to **`scripts/codebuild-*.sh`** for selective multi-image pushes (see file header comments).
- **`TaskFlow/buildspec.yml`** publishes **`dist/`** to **`s3://frontend.taskkfloww.shop`** and invalidates CloudFront **`E2RGM9XABY19D9`** (adjust ARNs/names if your account differs).

## GitOps Runtime Flow

```mermaid
sequenceDiagram
  participant Dev as Developer
  participant GH as GitHubActions
  participant ECR as Amazon_ECR
  participant EC2 as EC2_host
  participant Cmp as Docker_Compose
  participant CF as CloudFront_optional

  Dev->>GH: push TaskFlow_Server paths on develop
  GH->>GH: docker build main_service envoy
  GH->>ECR: push latest tags
  GH->>EC2: SSH pull_images_and_up_d

  EC2->>Cmp: compose_prod_pull_up_grpc_service_envoy

  Dev->>GH: push TaskFlow FE optional_CodeBuild
  GH->>CF: S3_sync_and_invalidation_optional
```

## Prerequisites

Local workstation:

- Git
- Docker and Docker Compose plugin
- **pnpm** (frontend)
- Node.js **22+** (frontend CI tests **22.x** and **24.x**)
- **.NET SDK 10** (for **`main-service`** outside containers — matches **`TargetFramework`** `net10.0`)
- Optional: Go tooling if you extend **`metrics-service`**
- AWS CLI v2 (deploy/templates and ECR checks)

AWS:

- AWS account with permissions for **CloudFormation**, **ECR**, **EC2**, **IAM**, **VPC**.
- ECR repositories used by workflows/spec comments:
  - **`main-service`**
  - **`envoy-proxy`**
  - **`notification-service`** (referenced by **`TaskFlow-Server/buildspec.yml`** comments when using CodeBuild)

Git repository layout caveat:

- GitHub executes workflows only from **`.github/workflows` at that repository’s root**. If **`Task`** is a single umbrella repo with nested **`TaskFlow/.git`** / **`TaskFlow-Server/.git`**, treat **`TaskFlow`** and **`TaskFlow-Server`** as **separate remotes**, or relocate workflows under the umbrella root.

## Run Locally

### Frontend (`TaskFlow/`)

```bash
cd TaskFlow
pnpm install
pnpm run dev
```

Default dev server is Vite (**often `http://localhost:5173`** — confirm from CLI output).

### Backend (`TaskFlow-Server/`)

Use Compose **profiles** from [TaskFlow-Server/docker-compose.yml](TaskFlow-Server/docker-compose.yml). Typical combinations depend on what you need (gRPC API, notifications, Kafka, Redis, Elasticsearch). Service ports and responsibilities are summarized in [TaskFlow-Server/README.md](TaskFlow-Server/README.md).

Example shape (adjust profiles and env files to match your setup):

```bash
cd TaskFlow-Server
docker compose --profile grpc_service --profile kafka --profile redis up --build
```

Env files such as **`.env`** / **`.env.dev`** are documented alongside the server README; **do not commit secrets**.

## Security Stack Setup

No bundled Nexus/Sonar/Trivy Compose ships with this umbrella README path. Treat security posture as:

| Topic | Guidance |
|---|---|
| CI secrets | Store **`AWS_*`**, **`EC2_*`**, **`REDIS_CONNECTIONSTRING`**, and SSH keys only in **GitHub Actions secrets** for the repository that owns the workflow. |
| Runtime secrets | Keep production **`TaskFlow-Server/.env`** on EC2 out of Git; inject **`ECR_REGISTRY`** for **`docker-compose.prod.yml`**. |
| Keys | Prefer **IAM roles** on EC2 instead of long-lived access keys where possible. Never commit **`*.csv`** access key bundles or checked-in `.env` with production passwords. |

Optional future alignment with the badges (Sonar/Trivy/Kyverno/Falco) would live outside this repo unless you add manifests later.

## EC2 Build Host Setup

The workflow connects with **`appleboy/ssh-action`** using **`EC2_USERNAME`** (often **`ubuntu`** — align with your AMI).

Install baseline tooling if not covered entirely by **`cloud-infrastructure/compute.yaml`** user data:

```bash
sudo apt update
sudo apt install -y git curl unzip ca-certificates
```

Docker / Compose:

```bash
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker "$USER"
docker compose version
```

AWS CLI v2 (needed on host for **`aws ecr get-login-password`** inside deploy script):

```bash
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o awscliv2.zip
unzip awscliv2.zip
sudo ./aws/install
aws --version
```

Ensure **`~/TaskFlow-Server`** exists and matches this Compose layout (`docker-compose.yml` + `docker-compose.prod.yml`, `.env` with **`ECR_REGISTRY`**).

Minimum IAM permissions for pushing/pulling images mirror standard **ECR push/pull** patterns when using instance roles instead of keys embedded in **`aws configure`**.

Validate:

```bash
git --version
docker --version
aws sts get-caller-identity
aws ecr get-login-password --region "$AWS_REGION"
```

## GitHub Actions Secrets

Configure secrets on the **`TaskFlow-Server`** GitHub repository (the one containing `.github/workflows/deploy-main_service-ecr.yml`):

```text
Repository > Settings > Secrets and variables > Actions
```

Required secrets:

| Secret | Required | Purpose |
|---|---:|---|
| `AWS_ACCESS_KEY_ID` | Yes | AWS credential for **`configure-aws-credentials`** and docker build-args where used. |
| `AWS_SECRET_ACCESS_KEY` | Yes | AWS secret key counterpart. |
| `AWS_REGION` | Yes | Region for ECR login and STS-derived registry URL on EC2. |
| `REDIS_CONNECTIONSTRING` | Yes | Passed into **`main-service`** Docker build as **`REDIS_CONNECTIONSTRING`** build-arg (workflow assumes presence). |
| `EC2_HOST` | Yes | Target host for SSH deploy. |
| `EC2_USERNAME` | Yes | SSH user on EC2 (commonly **`ubuntu`**). |
| `EC2_SSH_KEY` | Yes | Private key matching EC2 **`authorized_keys`**. |

`EC2_SSH_KEY` must contain the complete private key:

```text
-----BEGIN OPENSSH PRIVATE KEY-----
...
-----END OPENSSH PRIVATE KEY-----
```

The matching public key must exist on EC2:

```bash
/home/$USER/.ssh/authorized_keys
```

Frontend **`TaskFlow`** CI does **not** require AWS secrets for **`node.js.yml`** only.

## ECR, EKS, and Argo CD Setup

This project **does not use EKS or Argo CD** today; backend rollout uses **ECR images + Docker Compose on EC2**. CloudFormation templates live under **`cloud-infrastructure/`**.

### CloudFormation

Package/deploy nested stacks from **`cloud-infrastructure/`** (adjust bucket/prefix and capabilities for your account):

```bash
cd cloud-infrastructure
aws cloudformation deploy \
  --stack-name taskflow-root \
  --template-file root.yaml \
  --capabilities CAPABILITY_NAMED_IAM
```

Templates reference **`networking.yaml`** and **`compute.yaml`** via **`TemplateURL`** (upload nested templates to S3 in real deployments if required by your packaging workflow).

### ECR repositories

Create repositories if missing (region example **`ap-southeast-1`** from **`TaskFlow-Server/buildspec.yml`** comments — replace with **`AWS_REGION`**):

```bash
aws ecr create-repository --repository-name main-service --region "$AWS_REGION"
aws ecr create-repository --repository-name envoy-proxy --region "$AWS_REGION"
aws ecr create-repository --repository-name notification-service --region "$AWS_REGION"
```

### EC2 runtime with Compose

On EC2:

```bash
cd ~/TaskFlow-Server
export ECR_REGISTRY="<account>.dkr.ecr.<region>.amazonaws.com"
docker compose -f docker-compose.yml -f docker-compose.prod.yml pull
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

Ensure `.env` supplies credentials expected by **`main-service`** / **`notification-service`** images per environment docs.

### Frontend static hosting (optional CodeBuild path)

When using **`TaskFlow/buildspec.yml`**, configure CodeBuild IAM for **`s3:PutObject`** / **`cloudfront:CreateInvalidation`** on the documented bucket/distribution IDs.

## Pipeline Readiness Checklist

Before relying on **`deploy-main_service-ecr.yml`**:

- EC2 reachable via **`ssh $EC2_USERNAME@$EC2_HOST`** using the same key as **`EC2_SSH_KEY`**.
- **`~/TaskFlow-Server`** exists with **`docker-compose.yml`**, **`docker-compose.prod.yml`**, and production **`.env`** including **`ECR_REGISTRY`**.
- EC2 identity (`aws sts`) can **`docker login`** and **`pull`** from **`main-service`** / **`envoy-proxy`** repos.
- Repositories **`main-service`** and **`envoy-proxy`** exist in **`AWS_REGION`**.
- GitHub secrets **`AWS_*`**, **`EC2_*`**, **`REDIS_CONNECTIONSTRING`** are populated.
- Workflow triggers match **`develop`** commits touching filtered paths.

For SPA CodeBuild (**`TaskFlow/buildspec.yml`**):

- IAM allows **`aws s3 sync`** on **`frontend.taskkfloww.shop`** and CloudFront invalidation **E2RGM9XABY19D9** (or updated IDs).

## Troubleshooting

| Symptom | Common cause | Fix |
|---|---|---|
| `Permission denied (publickey)` | SSH key mismatch | Verify **`EC2_SSH_KEY`**, **`EC2_USERNAME`**, **`EC2_HOST`**, and **`authorized_keys`**. |
| Workflow skipped builds | Paths filter missed changes | Confirm edits landed under **`services/main-service/**`**, **`protos/**`**, **`envoy.yaml`**, or root **`dockerfile`**. |
| `docker login` fails on EC2 | Wrong IAM / STS identity | Attach EC2 role or credentials allowing **`ecr:GetAuthorizationToken`** and repo pulls. |
| Compose cannot pull **`main-service`** | Wrong **`ECR_REGISTRY`** / repo name | Match **`docker-compose.prod.yml`** image URIs with actual registry URIs. |
| **`grpc-service`** unhealthy | Missing infra profiles (.env / Kafka / Redis) | Start needed Compose profiles or validate `.env`. |
| Frontend CodeBuild denied | IAM lacking S3/CloudFront | Extend CodeBuild role per **`TaskFlow/buildspec.yml`** header comments. |
| Umbrella repo workflows silent | Nested `.github` paths | Ensure workflows live at repo root when GitHub repo == **`Task`** folder. |

## Security Practices Used

- Do **not** commit **AWS access key CSV files**, production `.env`, or private SSH keys.
- Prefer IAM roles on EC2 over embedding **`AWS_ACCESS_KEY_ID`** inside images where feasible.
- Envoy admin **`9901`** is intentionally **not** exposed in **`docker-compose.prod.yml`** comments — keep admin ports internal only.

## Documentation Index

| Document | Content |
|---|---|
| [TaskFlow/README.md](TaskFlow/README.md) | SPA toolchain notes (Vite template README). |
| [TaskFlow-Server/README.md](TaskFlow-Server/README.md) | Service ports, tech stack, Compose-oriented overview. |
| [TaskFlow-Server/services/nest-service/README.md](TaskFlow-Server/services/nest-service/README.md) | NestJS notification-service docs. |

## Expected Result

When **`TaskFlow-Server`** Actions succeed after a qualifying **`develop`** push:

```text
Source_code -> GitHubActions_build -> ECR_push_latest -> SSH_EC2 -> compose_pull_up -> running Envoy_and_grpc_service
```

Final validation commands (examples):

```bash
ssh "$EC2_USERNAME@$EC2_HOST" 'cd ~/TaskFlow-Server && sudo docker compose -f docker-compose.yml -f docker-compose.prod.yml ps'
aws ecr describe-images --repository-name main-service --region "$AWS_REGION"
aws ecr describe-images --repository-name envoy-proxy --region "$AWS_REGION"
```

Optional SPA validation after **`TaskFlow/buildspec.yml`** pipeline:

```bash
aws cloudfront get-invalidation --distribution-id E2RGM9XABY19D9 --id "<invalidation-id>"
```
