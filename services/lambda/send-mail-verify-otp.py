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


def _extract_envelope(record_or_event):
    """
    Supports both:
    - SNS trigger directly (event has Type/Message)
    - SQS trigger carrying SNS envelope in record.body
    """
    if isinstance(record_or_event, dict) and record_or_event.get("body") is not None:
        return _maybe_json_loads(record_or_event.get("body")) or {}
    return record_or_event if isinstance(record_or_event, dict) else {}


def _extract_verify_otp_payload(envelope):
    subject = envelope.get("Subject") or "Verify OTP"
    message = _maybe_json_loads(envelope.get("Message")) or {}

    event_type = message.get("eventType")
    data = message.get("data") or {}
    nested_data = data.get("data") or {}

    # OTP and recipient live under data.data (see MAILS_SEND_VERIFY_OTP_USER payload).
    otp = nested_data.get("otp")
    user_id = data.get("userId")

    # Prefer nested: recipientEmail, email; also accept typo "recipentEmail" from publisher.
    recipient_email = (
        nested_data.get("recipientEmail")
        or nested_data.get("recipentEmail")
        or nested_data.get("email")
        or data.get("recipientEmail")
        or data.get("email")
        or data.get("userEmail")
        or os.environ.get("DEFAULT_TO_EMAIL")
    )

    return {
        "subject": subject,
        "event_type": event_type,
        "otp": otp,
        "user_id": user_id,
        "recipient_email": recipient_email,
    }


def lambda_handler(event, context):
    ses_region = os.environ.get("AWS_REGION")
    client = boto3.client("ses", region_name=ses_region)

    source_email = os.environ.get("SES_SOURCE_EMAIL") or "n.nducthangg@gmail.com"

    # Normalize input list (SQS records or direct SNS event).
    entries = event.get("Records") if isinstance(event, dict) and event.get("Records") else [event]

    processed = 0
    last_message_id = None

    for entry in entries:
        envelope = _extract_envelope(entry)
        payload = _extract_verify_otp_payload(envelope)

        if payload.get("event_type") not in (None, "MAILS_SEND_VERIFY_OTP_USER"):
            print("Skip unsupported eventType", payload.get("event_type"))
            continue

        recipient_email = payload.get("recipient_email")
        otp = payload.get("otp")

        if not recipient_email:
            print("Skip record: no recipient email in payload or DEFAULT_TO_EMAIL.")
            continue

        if not otp:
            print("Skip record: missing otp in payload.")
            continue

        html_body = f"""
      <html>
  <body style="margin:0; padding: 2rem; background-color: #f4f4f5; font-family: Arial, sans-serif;">
    <div style="max-width: 480px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; border: 1px solid #e4e4e7;">

      <!-- Header -->
      <div style="background: #1a1a2e; padding: 1.5rem 2rem; display: flex; align-items: center; gap: 10px;">
        <div style="width: 28px; height: 28px; border-radius: 6px; background: #6C63FF; display: flex; align-items: center; justify-content: center;">
          <img src="https://img.icons8.com/ios-filled/14/ffffff/checkmark.png" width="14" height="14" />
        </div>
        <span style="color: white; font-weight: bold; font-size: 15px;">TaskFlow</span>
      </div>

      <!-- Body -->
      <div style="padding: 2rem;">
        <p style="font-size: 12px; color: #888; margin: 0 0 0.5rem; text-transform: uppercase; letter-spacing: 0.8px;">Security Code</p>
        <h2 style="font-size: 20px; font-weight: bold; color: #111; margin: 0 0 0.5rem;">Verify your account</h2>
        <p style="font-size: 14px; color: #555; margin: 0 0 1.75rem; line-height: 1.6;">
          Use the code below to complete your verification. This code is valid for <strong>10 minutes</strong> and should not be shared with anyone.
        </p>

        <!-- OTP Box -->
        <div style="background: #f9f9fb; border-radius: 8px; padding: 1.25rem; text-align: center; margin-bottom: 1.75rem; border: 1px solid #e4e4e7;">
          <p style="font-size: 11px; color: #999; margin: 0 0 0.4rem; letter-spacing: 0.5px;">YOUR OTP CODE</p>
          <p style="font-size: 32px; font-weight: bold; letter-spacing: 10px; color: #1a1a2e; margin: 0; font-family: monospace;">{otp}</p>
        </div>

        <!-- User ID -->
        <div style="border: 1px solid #e4e4e7; border-radius: 8px; padding: 0.75rem 1rem; display: flex; align-items: center; gap: 10px; margin-bottom: 1.75rem;">
          <div style="width: 28px; height: 28px; border-radius: 50%; background: #f4f4f5; display: flex; align-items: center; justify-content: center; flex-shrink: 0; font-size: 14px;">👤</div>
          <div>
            <p style="font-size: 11px; color: #999; margin: 0; letter-spacing: 0.3px;">USER ID</p>
            <p style="font-size: 13px; color: #111; margin: 0; font-family: monospace;">{payload.get("user_id") or ""}</p>
          </div>
        </div>

        <!-- Disclaimer -->
        <div style="border-top: 1px solid #e4e4e7; padding-top: 1.25rem;">
          <p style="font-size: 12px; color: #999; margin: 0; line-height: 1.7;">
            If you didn't request this code, you can safely ignore this email.
          </p>
        </div>
      </div>

      <!-- Footer -->
      <div style="background: #f9f9fb; border-top: 1px solid #e4e4e7; padding: 1rem 2rem; text-align: center;">
        <p style="font-size: 11px; color: #aaa; margin: 0;">© 2025 TaskFlow · Privacy Policy</p>
      </div>

    </div>
  </body>
</html>
        """.strip()

        text_body = f"Your TaskFlow OTP code is: {otp}"

        message = {
            "Subject": {"Data": payload.get("subject") or "Verify OTP"},
            "Body": {
                "Text": {"Data": text_body},
                "Html": {"Data": html_body},
            },
        }

        print("Sending verify OTP email", {"to": recipient_email, "subject": payload.get("subject")})
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
