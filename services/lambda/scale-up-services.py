"""
Start ECS services: restore desiredCount from SSM or DEFAULT_DESIRED_COUNTS.
Deploy alone: ./scripts/deploy-lambda.sh ecs-start
"""

from __future__ import annotations

import json
import logging
import os
from typing import Any

import boto3

logger = logging.getLogger()
logger.setLevel(logging.INFO)

ecs = boto3.client("ecs")
ssm = boto3.client("ssm")


def _cluster() -> str:
    return os.environ["ECS_CLUSTER_NAME"]


def _ssm_prefix() -> str:
    return os.environ.get("SSM_PREFIX", "/taskflow/ecs-scheduler").rstrip("/")


def _ssm_key(service: str) -> str:
    return f"{_ssm_prefix()}/{_cluster()}/{service}/desired-count"


def _allowlist() -> list[str] | None:
    raw = os.environ.get("ECS_SERVICE_NAMES", "").strip()
    if not raw:
        return None
    return [s.strip() for s in raw.split(",") if s.strip()]


def _defaults() -> dict[str, int]:
    raw = os.environ.get("DEFAULT_DESIRED_COUNTS", "{}")
    return {str(k): int(v) for k, v in json.loads(raw).items()}


def _list_services() -> list[str]:
    cluster = _cluster()
    allowlist = _allowlist()
    arns: list[str] = []
    token: str | None = None
    while True:
        kwargs: dict[str, Any] = {"cluster": cluster}
        if token:
            kwargs["nextToken"] = token
        resp = ecs.list_services(**kwargs)
        arns.extend(resp.get("serviceArns", []))
        token = resp.get("nextToken")
        if not token:
            break
    names = [arn.rsplit("/", 1)[-1] for arn in arns]
    if allowlist:
        missing = set(allowlist) - set(names)
        if missing:
            raise ValueError(f"Services not found in {cluster}: {sorted(missing)}")
        return allowlist
    return sorted(names)


def _describe_counts(services: list[str]) -> dict[str, int]:
    if not services:
        return {}
    resp = ecs.describe_services(cluster=_cluster(), services=services)
    return {s["serviceName"]: int(s.get("desiredCount", 0)) for s in resp.get("services", [])}


def _target_count(service: str, defaults: dict[str, int]) -> int:
    try:
        return int(ssm.get_parameter(Name=_ssm_key(service))["Parameter"]["Value"])
    except ssm.exceptions.ParameterNotFound:
        return defaults.get(service, 1)


def lambda_handler(event: dict[str, Any], context: Any) -> dict[str, Any]:
    services = _list_services()
    defaults = _defaults()
    before = _describe_counts(services)
    started: list[dict[str, Any]] = []

    for name in services:
        target = _target_count(name, defaults)
        if before.get(name, 0) == target:
            continue
        ecs.update_service(cluster=_cluster(), service=name, desiredCount=target)
        started.append(
            {
                "service": name,
                "desiredCount": target,
                "previousDesiredCount": before.get(name, 0),
            }
        )
        logger.info("Started %s -> %s", name, target)

    body = {"action": "start", "started": started}
    logger.info("%s", json.dumps(body))
    return {"statusCode": 200, "body": body}
