package config

import (
	"fmt"

	"github.com/spf13/viper"
)

type Config struct {
	App AppConfig `yaml:"app"`
}
type AppConfig struct {
	Name string `yaml:"name"`
	Port int    `yaml:"port"`
	Env  string `yaml:"env"`
}

func LoadConfig(serviceName string) (*Config, error) {
	v := viper.New()
	v.SetConfigName("config")
	v.SetConfigType("yaml")
	v.AddConfigPath(fmt.Sprintf("./config/%s", serviceName)) // ./config/user-service/config.yaml
	v.AddConfigPath(".")

	v.AutomaticEnv()
	if err := v.ReadInConfig(); err != nil {
		return nil, fmt.Errorf("error reading config for %s: %w", serviceName, err)
	}
	var cfg Config
	if err := v.Unmarshal(&cfg); err != nil {
		return nil, fmt.Errorf("error unmarshaling config: %w", err)
	}
	return &cfg, nil
}
