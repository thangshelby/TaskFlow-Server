package services

import "github.com/vudinhan2525/TaskFlow-Server/services/go-service/internal/media-service/core/interfaces"

type MediaService struct {
	repo interfaces.IMediaRepo
}

func NewMediaService(repo interfaces.IMediaRepo) *MediaService {
	return &MediaService{repo: repo}
}

func (s *MediaService) HandleMedia(data string) error {
	return s.repo.SaveMedia(data)
}
