package adapter

import (
	"context"

	"github.com/vudinhan2525/TaskFlow-Server/services/go-service/internal/media-service/core/services"
	"github.com/vudinhan2525/TaskFlow-Server/services/go-service/types/media_service"
)

type GRPCServer struct {
	media_service.UnimplementedMediaServiceServer
	mediaService *services.MediaService
}

func NewGRPCServer(mediaService *services.MediaService) *GRPCServer {
	return &GRPCServer{mediaService: mediaService}
}

func (s *GRPCServer) UploadImage(ctx context.Context, req *media_service.UploadImageReq) (*media_service.UploadImageRes, error) {
	// result, err := s.mediaService.HandleUpload(ctx, req)
	// if err != nil {
	// 	return nil, err
	// }

	return &media_service.UploadImageRes{
		Url: "xxx",
	}, nil
}
