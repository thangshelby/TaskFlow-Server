"""
Stop ECS services (desiredCount=0) and save previous counts to SSM.
Deploy alone: ./scripts/deploy-lambda.sh ecs-stop
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


def lambda_handler(event: dict[str, Any], context: Any) -> dict[str, Any]:
    services = _list_services()
    current = _describe_counts(services)
    stopped: list[dict[str, Any]] = []
    skipped: list[str] = []

    for name in services:
        prev = current.get(name, 0)
        if prev == 0:
            skipped.append(name)
            continue
        ssm.put_parameter(
            Name=_ssm_key(name),
            Value=str(prev),
            Type="String",
            Overwrite=True,
        )
        ecs.update_service(cluster=_cluster(), service=name, desiredCount=0)
        stopped.append({"service": name, "previousDesiredCount": prev})
        logger.info("Stopped %s (was %s)", name, prev)

    body = {"action": "stop", "stopped": stopped, "skippedAlreadyZero": skipped}
    logger.info("%s", json.dumps(body))
    return {"statusCode": 200, "body": body}
