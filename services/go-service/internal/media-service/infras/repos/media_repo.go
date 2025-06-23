package repos

import "github.com/vudinhan2525/TaskFlow-Server/services/go-service/internal/media-service/core/interfaces"

type MediaRepo struct{}

func NewMediaRepo() *MediaRepo {
	return &MediaRepo{}
}

func (r *MediaRepo) SaveMedia(data string) error {
	// Save logic (mocked for now)
	return nil
}

var _ interfaces.IMediaRepo = (*MediaRepo)(nil)
