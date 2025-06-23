//go:build wireinject
// +build wireinject

package adapter

import (
	"github.com/google/wire"
	"github.com/vudinhan2525/TaskFlow-Server/services/go-service/internal/media-service/core/interfaces"
	"github.com/vudinhan2525/TaskFlow-Server/services/go-service/internal/media-service/core/services"
	"github.com/vudinhan2525/TaskFlow-Server/services/go-service/internal/media-service/infras/repos"
)

func InitializeGRPCServer() (*GRPCServer, error) {
	wire.Build(
		repos.NewMediaRepo,
		wire.Bind(new(interfaces.IMediaRepo), new(*repos.MediaRepo)),
		services.NewMediaService,
		NewGRPCServer,
	)
	return &GRPCServer{}, nil
}
