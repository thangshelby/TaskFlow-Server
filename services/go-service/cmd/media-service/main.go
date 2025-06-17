package main

import (
	"net"

	"github.com/vudinhan2525/TaskFlow-Server/services/go-service/pkg/config"
	"github.com/vudinhan2525/TaskFlow-Server/services/go-service/pkg/log"
	"github.com/vudinhan2525/TaskFlow-Server/services/go-service/types/media_service"
	"google.golang.org/grpc"
	"google.golang.org/grpc/reflection"
)

type Server struct {
	media_service.UnimplementedMediaServiceServer
	Config config.Config
}

func NewServer(config config.Config) (*Server, error) {

	server := Server{Config: config}
	return &server, nil
}

func main() {
	cfg, err := config.LoadConfig("media-service")

	if err != nil {
		log.Logger.Info("failed to load config", err)
	}

	server, err := NewServer(*cfg)
	if err != nil {
		log.Logger.Fatal("Error when creating server")
	}

	grpcServer := grpc.NewServer()
	media_service.RegisterMediaServiceServer(grpcServer, server)
	reflection.Register(grpcServer)

	listener, err := net.Listen("tcp", "127.0.0.1:5003")
	if err != nil {
		log.Logger.Fatal("Error when creating listener")
	}
	log.Logger.Printf("start gRPC server at %s", listener.Addr().String())

	err = grpcServer.Serve(listener)
	if err != nil {
		log.Logger.Fatal("Cannot creating grpc server")
	}
}
