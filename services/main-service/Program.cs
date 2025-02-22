using MainService.Domain.Interfaces;
using MainService.Domain.UseCases;
using MainService.Infras;
using MainService.Infras.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configure Services
builder.Services.AddSingleton<MongoDbService>();

// Add Swagger and Controllers
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Register DI
builder.Services.AddScoped<UserUseCase>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


var app = builder.Build();

// Check connection
app.Services.GetRequiredService<MongoDbService>();


// Configure Middleware
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Test Endpoint
app.MapGet("/health/test", () => "Live!!");

// Register Controllers
app.MapControllers();

app.Run();
