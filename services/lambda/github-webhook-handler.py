import hashlib, hmac, json, os, boto3, uuid, re

QUEUE_TOPIC_NAME = "notifications"
QUEUE_MESSAGE_ACTION = "NOTIFICATIONS_CREATE_NEW_NOTIFICATION"

def extract_issue_keys(text):
    if not text:
        return [None]
    # Find issue keys like TF-123
    keys = re.findall(r'\b([A-Za-z]+-\d+)\b', text)
    keys = list(set([k.upper() for k in keys]))
    return keys if keys else [None]

def handle_push_event(payload):
    commits = payload.get("commits", [])
    messages = []
    
    if not commits:
        pusher = payload.get("pusher", {})
        commits = [{
            "message": "Pushed to repository",
            "url": payload.get("repository", {}).get("html_url", ""),
            "author": pusher
        }]

    ref = payload.get("ref", "")
    branch_name = ref.replace("refs/heads/", "") if ref else ""

    for commit in commits:
        author = commit.get("author", {})
        commit_message = commit.get("message", "")
        author_name = author.get("name", "Someone")
        author_email = author.get("email", "")
        commit_url = commit.get("url", "")
        
        combined_text = f"{commit_message} {branch_name}"
        issue_keys = extract_issue_keys(combined_text)
        for key in issue_keys:
            msg = {
                "type": "GITHUB_COMMIT",
                "authorName": author_name,
                "authorEmail": author_email,
                "commitMessage": commit_message,
                "commitUrl": commit_url
            }
            if key:
                msg["issueKey"] = key
            messages.append(msg)
            
    return messages

def handle_pull_request_event(payload):
    action = payload.get("action")
    pr = payload.get("pull_request", {})
    messages = []
    
    # We support major actions: opened, reopened, closed, synchronize, edited
    if action in ["opened", "reopened", "closed", "synchronize", "edited"]:
        if action in ["opened", "reopened"]:
            event_type = "GITHUB_PR_OPENED"
        elif action == "closed":
            event_type = "GITHUB_PR_CLOSED"
        else:
            event_type = "GITHUB_PR_SYNCHRONIZE"
        
        user = pr.get("user", {})
        title = pr.get("title", "")
        body = pr.get("body", "") or ""
        pr_url = pr.get("html_url", "")
        branch_name = pr.get("head", {}).get("ref", "")
        
        author_name = user.get("login", "Someone")
        author_email = user.get("email", "")
        
        # Search for issue keys in title, body, and branch name
        combined_text = f"{title} {body} {branch_name}"
        issue_keys = extract_issue_keys(combined_text)
        
        for key in issue_keys:
            msg = {
                "type": event_type,
                "authorName": author_name,
                "authorEmail": author_email,
                "commitMessage": title, # we reuse commitMessage field for notification body/title
                "commitUrl": pr_url
            }
            if key:
                msg["issueKey"] = key
            messages.append(msg)
            
    return messages

def get_header(headers, target_key):
    if not headers:
        return None
    target_lower = target_key.lower()
    for k, v in headers.items():
        if k.lower() == target_lower:
            return v
    return None

def lambda_handler(event, context):
    headers = event.get("headers", {}) or {}
    # 1. Bảo mật: Xác thực GitHub Signature
    secret = os.environ.get("GITHUB_WEBHOOK_SECRET", "").encode()
    body = event.get("body", "")
    sig = get_header(headers, "x-hub-signature-256")
    
    if secret:
        expected = "sha256=" + hmac.new(secret, body.encode('utf-8'), hashlib.sha256).hexdigest()
        if not hmac.compare_digest(sig or "", expected):
            print("Invalid signature detected!")
            # return {"statusCode": 401, "body": "Invalid signature"}

    payload = json.loads(body)
    gh_event = get_header(headers, "x-github-event")
    print("GitHub Event:", gh_event)
    print("Payload keys:", list(payload.keys()))
    print("Action in payload:", payload.get("action"))
    print("Has pull_request key:", "pull_request" in payload)
    
    messages = []
    
    if gh_event == "push":
        messages = handle_push_event(payload)
    elif gh_event == "pull_request":
        messages = handle_pull_request_event(payload)
        
    if not messages:
        print("Event ignored or no messages generated")
        return {"statusCode": 200, "body": "Event ignored or no messages generated"}
        
    topic_arn = os.environ.get('SNS_TOPIC_ARN')
    if not topic_arn:
        print("SNS_TOPIC_ARN environment variable is not defined")
        return {"statusCode": 500, "body": "SNS Topic ARN missing"}
        
    sns_client = boto3.client("sns")
    
    for msg_data in messages:
        envelope = {
            "id": str(uuid.uuid4()),
            "eventType": QUEUE_MESSAGE_ACTION,
            "data": msg_data
        }
        
        print("Publishing notification envelope to SNS:", json.dumps(envelope))
        
        params = {
            'TopicArn': topic_arn,
            'Message': json.dumps(envelope),
            'MessageGroupId': QUEUE_TOPIC_NAME,
            'MessageDeduplicationId': str(uuid.uuid4()),
            'MessageAttributes': {
                'Action': {
                    'DataType': 'String',
                    'StringValue': QUEUE_MESSAGE_ACTION
                },
                'Topic': {
                    'DataType': 'String',
                    'StringValue': QUEUE_TOPIC_NAME
                }
            }
        }
        response = sns_client.publish(**params)
        print(f"Published message {response['MessageId']} to SNS topic {topic_arn}")
        
    return {"statusCode": 200, "body": f"Processed {len(messages)} messages"}