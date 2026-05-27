import hashlib, hmac, json, os, boto3

def lambda_handler(event, context):
    # 1. Bảo mật: Xác thực GitHub Signature
    secret = os.environ.get("GITHUB_WEBHOOK_SECRET", "").encode()
    body = event.get("body", "")
    # Lưu ý: HTTP API của AWS thường trả về 'x-hub-signature-256' chữ thường
    sig = event.get("headers", {}).get("x-hub-signature-256", "")
    
    if secret:
        expected = "sha256=" + hmac.new(secret, body.encode('utf-8'), hashlib.sha256).hexdigest()
        if not hmac.compare_digest(sig, expected):
            print("Invalid signature detected!")
            # return {"statusCode": 401, "body": "Invalid signature"}

    # 2. Xử lý logic
    payload = json.loads(body)
    gh_event = event["headers"].get("x-github-event")
    
    if gh_event :
        action = payload.get("action")
        print("action: ",action)
        print("payload: ",payload)
        # Chỉ xử lý khi PR được mở hoặc cập nhật
        # if action in ["opened", "synchronize", "closed"]:
        #     pr_data = {
        #         "pr_id": payload["pull_request"]["id"],
        #         "title": payload["pull_request"]["title"],
        #         "url": payload["pull_request"]["html_url"],
        #         "sender": payload["sender"]["login"],
        #         "action": action
        #     }
            
        #     # 3. Gửi vào SQS
        #     sqs = boto3.client("sqs")
        #     sqs.send_message(
        #         QueueUrl=os.environ["SQS_QUEUE_URL"],
        #         MessageBody=json.dumps(pr_data)
        #     )
        #     print(f"Sent PR {pr_data['pr_id']} to SQS")

    return {"statusCode": 200, "body": "Processed"}