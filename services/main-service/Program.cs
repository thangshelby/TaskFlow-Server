
using Serilog;
using MainService.Domain.Interfaces;
using MainService.Domain.UseCases;
using MainService.Infras;
using MainService.Infras.Repositories;


var builder = WebApplication.CreateBuilder(args);

// Configure Services
builder.Services.AddSingleton<MongoDbService>();

// Add Swagger and Controllers
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddCarter();

builder.Services.AddGrpc();

builder.Services.AddGrpcReflection();

// Register DI
builder.Services.AddScoped<UserUseCase>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, projectRepository>();
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));


builder.Services.AddAutoMapper(typeof(Program));


var app = builder.Build();
app.UseSerilogRequestLogging();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var kestrelUrl = builder.Configuration.GetValue<string>("Kestrel:Endpoints:Http:Url");
logger.LogInformation("🚀 GRPC server starting on {Addresses}", kestrelUrl);



// Check connection
app.Services.GetRequiredService<MongoDbService>();



// Configure Middleware
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

// Map GRPC
app.MapGrpcService<ProjectServiceImpl>(); // Map the service
app.MapGrpcService<CommentServiceImpl>(); // Map the service
if (app.Environment.IsDevelopment())
{
  app.MapGrpcReflectionService();
}
app.MapGet("/", () => "This is a gRPC service. Use a gRPC client to communicate.");



app.Run();
