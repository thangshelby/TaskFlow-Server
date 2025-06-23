package main

import (
	"net"

	adapter "github.com/vudinhan2525/TaskFlow-Server/services/go-service/internal/media-service/adapter"
	"github.com/vudinhan2525/TaskFlow-Server/services/go-service/pkg/config"
	interceptor "github.com/vudinhan2525/TaskFlow-Server/services/go-service/pkg/interceptors"
	"github.com/vudinhan2525/TaskFlow-Server/services/go-service/pkg/log"
	"github.com/vudinhan2525/TaskFlow-Server/services/go-service/types/media_service"
	"google.golang.org/grpc"
	"google.golang.org/grpc/reflection"
)

func main() {
	cfg, err := config.LoadConfig("media-service")

	if err != nil {
		log.Logger.Info("failed to load config", err)
	}

	grpcHandler, err := adapter.InitializeGRPCServer()
	if err != nil {
		log.Logger.Fatal("Error when creating server")
	}

	grpcServer := grpc.NewServer(grpc.ChainUnaryInterceptor(interceptor.AuthUnaryInterceptor, interceptor.GlobalUnaryErrorInterceptor))
	media_service.RegisterMediaServiceServer(grpcServer, grpcHandler)
	reflection.Register(grpcServer)

	listener, err := net.Listen("tcp", cfg.App.Port)
	if err != nil {
		log.Logger.Fatal("Error when creating listener")
	}
	log.Logger.Printf("start gRPC server at %s", listener.Addr().String())

	err = grpcServer.Serve(listener)
	if err != nil {
		log.Logger.Fatal("Cannot creating grpc server")
	}
}
