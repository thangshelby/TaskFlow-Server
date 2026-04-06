
import json
import os
import boto3


def _maybe_json_loads(value):
    if value is None:
        return None
    if isinstance(value, (dict, list)):
        return value
    if isinstance(value, str):
        try:
            return json.loads(value)
        except json.JSONDecodeError:
            return None
    return None


def _extract_notification_payload(record):
    """
    Expecting SQS record whose `body` is an SNS notification envelope.
    Example (simplified):
      record.body = {
        "Subject": "...",
        "Message": "{ \"eventType\": \"...\", \"data\": {...} }"
      }
    """
    record_body = record.get("body")
    envelope = _maybe_json_loads(record_body) or {}

    subject = envelope.get("Subject") or "TaskFlow notification"
    message_inner = _maybe_json_loads(envelope.get("Message")) or {}

    # Sometimes `Message` might already be a dict; this keeps it robust.
    if isinstance(message_inner, dict):
        event_type = message_inner.get("eventType")
        data = message_inner.get("data") or {}
    else:
        event_type = None
        data = {}

    return {
        "subject": subject,
        "eventType": event_type,
        "data": data,
    }


def _extract_recipient_email(data, fallback_email):
    """
    Best-effort extraction:
    - Prefer explicit `recipientEmail` in payload
    - If absent, treat `recipientId` as an email if it looks like one
    - Otherwise use a configured fallback
    """
    recipient_email = data.get("recipientEmail") or data.get("recipient_email")
    if recipient_email and isinstance(recipient_email, str) and "@" in recipient_email:
        return recipient_email

    recipient_id = data.get("recipientId")
    if recipient_id and isinstance(recipient_id, str) and "@" in recipient_id:
        return recipient_id

    return fallback_email

def lambda_handler(event, context):
    ses_region = os.environ.get("AWS_REGION")  # optional
    client = boto3.client("ses", region_name=ses_region)

    source_email = os.environ.get("SES_SOURCE_EMAIL") or "n.nducthangg@gmail.com"
    fallback_to_email = os.environ.get("DEFAULT_TO_EMAIL") or source_email

    records = event.get("Records") or []
    if not records:
        print("No SQS records to process.")
        return {"status": "no_records", "processed": 0}

    processed = 0
    last_message_id = None
    for record in records:
        notification = _extract_notification_payload(record)
        subject = notification.get("subject") or "TaskFlow notification"
        data = notification.get("data") or {}

        recipient_email = _extract_recipient_email(data, fallback_to_email)
        recipient_id = data.get("recipientId")
        actor_id = data.get("actorId")
        issue_id = data.get("issueId")
        notif_type = data.get("type") or notification.get("eventType")

        # Build a simple HTML email. You can later replace with a template.
        html_body = f"""
        <html>
          <body style="font-family: Arial, sans-serif;">
            <h3>TaskFlow: You have a new notification</h3>
            <p><b>Type</b>: {notif_type or ""}</p>
            <p><b>Issue</b>: {issue_id or ""}</p>
            <p><b>Actor</b>: {actor_id or ""}</p>
            <p style="color:#666;"><b>RecipientId</b>: {recipient_id or ""}</p>
          </body>
        </html>
        """.strip()

        message = {
            "Subject": {"Data": subject},
            "Body": {
                "Html": {"Data": html_body},
            },
        }

        print("Sending email", {"to": recipient_email, "subject": subject})
        send_response = client.send_email(
            Source=source_email,
            Destination={"ToAddresses": [recipient_email]},
            Message=message,
        )
        last_message_id = send_response.get("MessageId")
        processed += 1

    return {
        "status": "ok",
        "processed": processed,
        "lastMessageId": last_message_id,
    }