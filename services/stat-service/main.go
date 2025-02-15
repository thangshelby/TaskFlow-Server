package main

import (
	"net"

	"github.com/vudinhan2525/TaskFlow/gapi"
	"github.com/vudinhan2525/TaskFlow/pb"
	"github.com/vudinhan2525/TaskFlow/pkg/log"
	"github.com/vudinhan2525/TaskFlow/util"
	"google.golang.org/grpc"
	"google.golang.org/grpc/reflection"
)

func main() {
	config, err := util.LoadConfig(".")
	if err != nil {
		log.Logger.Fatal("Error when loading env!!", err)
	}
	runGrpcServer(config)
}
func runGrpcServer(config util.Config) {
	server, err := gapi.NewServer(config)
	if err != nil {
		log.Logger.Fatal("Error when creating server")
	}

	grpcServer := grpc.NewServer()
	pb.RegisterStatServiceServer(grpcServer, server)
	reflection.Register(grpcServer)

	listener, err := net.Listen("tcp", config.APIEndpoint)
	if err != nil {
		log.Logger.Fatal("Error when creating listener")
	}
	log.Logger.Printf("start gRPC server at %s", listener.Addr().String())

	err = grpcServer.Serve(listener)
	if err != nil {
		log.Logger.Fatal("Cannot creating grpc server")
	}
}
