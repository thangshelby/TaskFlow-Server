package interceptor

import (
	"context"
	"encoding/base64"
	"encoding/json"
	"errors"
	"log"
	"strings"

	"google.golang.org/grpc"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/metadata"
	"google.golang.org/grpc/status"
)

type JwtPayload struct {
	UserID string `json:"userId"`
	Role   string `json:"role"`
}

type ctxKey string

const (
	CtxUserID   ctxKey = "userId"
	CtxUserRole ctxKey = "userRole"
)

func AuthUnaryInterceptor(
	ctx context.Context,
	req interface{},
	info *grpc.UnaryServerInfo,
	handler grpc.UnaryHandler,
) (interface{}, error) {
	md, ok := metadata.FromIncomingContext(ctx)
	if !ok {
		return nil, status.Error(codes.Unauthenticated, "missing metadata")
	}
	rawToken := getFirst(md, "cookie")
	token := extractToken(rawToken)
	if token != "" {
		payload, err := decodeJWTToken(token)
		if err != nil {
			return nil, status.Errorf(codes.Unauthenticated, "token decode failed: %v", err)
		}

		ctx = context.WithValue(ctx, CtxUserID, payload.UserID)
		ctx = context.WithValue(ctx, CtxUserRole, payload.Role)

		log.Printf("Decoded JWT: userId=%s, role=%s", payload.UserID, payload.Role)

	}
	return handler(ctx, req)
}

func getFirst(metadata metadata.MD, key string) string {
	values := metadata.Get(key)
	if len(values) > 0 {
		return values[0]
	}
	return ""
}

func extractToken(rawToken string) string {
	parts := strings.Split(rawToken, ";")
	for _, part := range parts {
		trimmed := strings.TrimSpace(part)
		if strings.HasPrefix(trimmed, "token=") {
			return strings.TrimPrefix(trimmed, "token=")
		}
	}
	return ""
}

func decodeJWTToken(token string) (*JwtPayload, error) {
	parts := strings.Split(token, ".")
	if len(parts) < 2 {
		return nil, errors.New("invalid JWT format")
	}
	payloadSegments := parts[1]
	padded := padBase64(payloadSegments)

	decoded, err := base64.URLEncoding.DecodeString(padded)
	if err != nil {
		return nil, err
	}
	var payload JwtPayload
	if err := json.Unmarshal(decoded, &payload); err != nil {
		return nil, err
	}

	return &payload, nil
}

// padBase64 ensures the base64 string has the correct padding
func padBase64(s string) string {
	switch len(s) % 4 {
	case 2:
		return s + "=="
	case 3:
		return s + "="
	case 0:
		return s
	default:
		return s
	}
}
