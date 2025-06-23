package interceptor

import (
	"context"

	"github.com/vudinhan2525/TaskFlow-Server/services/go-service/pkg/log"
	"google.golang.org/grpc"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"
)

func GlobalUnaryErrorInterceptor(
	ctx context.Context,
	req interface{},
	info *grpc.UnaryServerInfo,
	handler grpc.UnaryHandler,
) (interface{}, error) {

	resp, err := handler(ctx, req)
	if err != nil {
		log.Logger.Printf("gRPC Error - Method: %s, Error: %v\n", info.FullMethod, err)

		return nil, status.Errorf(codes.Internal, "Internal Server Error: %v", err)
	}

	return resp, nil
}
