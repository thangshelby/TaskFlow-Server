package gapi

import (
	"github.com/vudinhan2525/TaskFlow/pb"
	"github.com/vudinhan2525/TaskFlow/util"
)

type Server struct {
	pb.UnimplementedStatServiceServer
	Config util.Config
}

func NewServer(config util.Config) (*Server, error) {

	server := Server{Config: config}
	return &server, nil
}
