FROM envoyproxy/envoy:v1.33.0

WORKDIR /etc/envoy

# Defaults for ECS/compose; override via env (see .env).
ENV GRPC_SERVICE_HOST=grpc-service
ENV NOTIFICATION_SERVICE_HOST=notification-service
ENV FRONTEND_URL=http://localhost:5173

# Keep proto and base config inside image.
COPY envoy.yaml /etc/envoy/envoy.yaml
COPY proto.pb /etc/envoy/proto.pb

# Render environment variables in envoy.yaml before starting Envoy.
CMD perl -pe 's/\$\{GRPC_SERVICE_HOST\}/$ENV{"GRPC_SERVICE_HOST"}/g; s/\$\{NOTIFICATION_SERVICE_HOST\}/$ENV{"NOTIFICATION_SERVICE_HOST"}/g; s/\$\{FRONTEND_URL\}/$ENV{"FRONTEND_URL"}/g;' /etc/envoy/envoy.yaml > /tmp/envoy.rendered.yaml && exec envoy -c /tmp/envoy.rendered.yaml