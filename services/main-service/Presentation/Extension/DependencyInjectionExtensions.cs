using FluentValidation;
using MainService.Domain.Interfaces;
using MainService.Domain.UseCases;
using MainService.Infras;
using MainService.Infras.Repositories;
using MainService.Presentation.Validator.Users;
using StackExchange.Redis;
using MainService.Domain.Decorator;
using MainService.Presentation.MiddleWare;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddProjectServices(this IServiceCollection services)
    {
        // Database Service
        services.AddSingleton<MongoDbService>();
        
        // Redis & Cache Service (conditional)
        services.AddRedisServices();
        
        // Use Cases
        services.AddScoped<UserUseCase>();
        services.AddScoped<ProjectUseCase>();
        services.AddScoped<SprintUseCase>();
        services.AddScoped<IssueUseCase>();
        services.AddScoped<ProjectMemberUseCase>();
        services.AddScoped<CommentUseCase>();
        services.AddScoped<OtpTokenUseCase>();
        services.AddScoped<ProjectTeamUseCase>();
        services.AddScoped<MetadataUseCase>();

        // Repositories
        services.AddSingleton<ITransactionRepo, MongoTransactionRepo>();
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IProjectRepository, ProjectRepository>();
        services.AddSingleton<ISprintRepository, SprintRepository>();
        // services.AddSingleton<IQueueRepository, KafkaRepository>();
        services.AddSingleton<IQueueRepository, AWSQueueRepository>();

        // Register IssueRepository with Cache Decorator
        services.AddSingleton<IssueRepository>();
        services.AddSingleton<IIssueRepository>(sp =>
        {
            var innerRepository = sp.GetRequiredService<IssueRepository>();
            var cacheRepository = sp.GetRequiredService<ICacheRepository>();
            var logger = sp.GetRequiredService<ILogger<IssueCacheDecorator>>();
            return new IssueCacheDecorator(innerRepository, cacheRepository, logger);
        });
        
        services.AddSingleton<IActivitiesRepository, ActivitiesRepository>();
        
        // Register ProjectMemberRepository with Cache Decorator
        services.AddSingleton<ProjectMemberRepository>();
        services.AddSingleton<IProjectMemberRepository>(sp =>
        {
            var innerRepository = sp.GetRequiredService<ProjectMemberRepository>();
            var cacheRepository = sp.GetRequiredService<ICacheRepository>();
            var logger = sp.GetRequiredService<ILogger<ProjectMemberCacheDecorator>>();
            return new ProjectMemberCacheDecorator(innerRepository, cacheRepository, logger);
        });
        services.AddSingleton<ICommentsRepository, CommentsRepository>();
        services.AddSingleton<IOtpTokenRepository, OtpTokenRepository>();
        services.AddSingleton<IPublisherService, QueuePublisher>();
        services.AddSingleton<IProjectTeamRepository, ProjectTeamRepository>();
        services.AddSingleton<IS3Repository, S3Repository>();

        // Workers
        services.AddHostedService<ActivitiesConsumer>();

        // Register Rate Limiter
        services.AddSingleton(new RateLimitOptions
        {
            Capacity = 100,
            RefillPerSecond = 20
        });

        return services;
    }

    /// <summary>
    /// Conditionally registers Redis-backed or in-memory/no-op services
    /// based on whether "Redis:ConnectionString" is configured.
    /// </summary>
    private static void AddRedisServices(this IServiceCollection services)
    {
        services.AddSingleton<ICacheRepository>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var redisConnection = configuration.GetValue<string>("Redis:ConnectionString");
            var composeProfiles = Environment.GetEnvironmentVariable("COMPOSE_PROFILES") ?? "";
            var isRedis = composeProfiles.Contains("redis");
            if (string.IsNullOrWhiteSpace(redisConnection) || !isRedis)
            {
                var logger = sp.GetRequiredService<ILogger<NoOpCacheRepository>>();
                return new NoOpCacheRepository(logger);
            }

            try
            {
                var redis = ConnectionMultiplexer.Connect(redisConnection);
                var logger = sp.GetRequiredService<ILogger<CacheRepository>>();
                logger.LogInformation("Connected to Redis at '{ConnectionString}'. Using CacheRepository.", redisConnection);
                return new CacheRepository(redis, logger);
            }
            catch (Exception ex)
            {
                var logger = sp.GetRequiredService<ILogger<NoOpCacheRepository>>();
                logger.LogWarning(ex, "Failed to connect to Redis at '{ConnectionString}'. Falling back to NoOp cache.", redisConnection);
                return new NoOpCacheRepository(logger);
            }
        });

        services.AddSingleton<IRateLimiter>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var redisConnection = configuration.GetValue<string>("Redis:ConnectionString");
            var options = sp.GetRequiredService<RateLimitOptions>();
            var composeProfiles = Environment.GetEnvironmentVariable("COMPOSE_PROFILES") ?? "";
            var isRedis = composeProfiles.Contains("redis");

            if (string.IsNullOrWhiteSpace(redisConnection) || !isRedis)
            {
                return new InMemoryRateLimiter(options);
            }

            try
            {
                var redis = ConnectionMultiplexer.Connect(redisConnection);
                return new RedisTokenBucketRateLimiter(redis, options);
            }
            catch (Exception ex)
            {
                var logger = sp.GetRequiredService<ILogger<InMemoryRateLimiter>>();
                logger.LogWarning(ex, "Failed to connect to Redis for rate limiter. Falling back to in-memory rate limiter.");
                return new InMemoryRateLimiter(options);
            }
        });
    }

    public static IServiceCollection AddGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<AuthenticationInterceptor>();
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        });
        services.AddGrpcReflection();

        return services;
    }

    public static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateProjectValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateProjectValidator>();

        services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
        services.AddValidatorsFromAssemblyContaining<LoginUserValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateUserValidator>();
        services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
        services.AddValidatorsFromAssemblyContaining<ChangePasswordValidator>();

        services.AddValidatorsFromAssemblyContaining<CreateIssueValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateIssueValidator>();

        return services;
    }
}
