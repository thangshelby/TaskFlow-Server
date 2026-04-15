# Use the official .NET 10 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Set working directory
WORKDIR /app

# Copy proto files first
COPY protos ./protos

# Copy project file
COPY services/main-service/NetService.csproj ./services/main-service/

# Restore dependencies
RUN dotnet restore services/main-service/NetService.csproj

# Copy the rest of the source code
COPY services/main-service/ ./services/main-service/

# Build the application
RUN dotnet build services/main-service/NetService.csproj -c Release

# Publish the application
RUN dotnet publish services/main-service/NetService.csproj -c Release -o /out

# Use the official .NET 10 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# Set working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /out .

# Expose the port that the app runs on
EXPOSE 5001

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://0.0.0.0:5001
ENV AWS_REGION=ap-southeast-1
ENV AWS_ACCESS_KEY_ID=${AWS_ACCESS_KEY_ID}
ENV AWS_SECRET_ACCESS_KEY=${AWS_SECRET_ACCESS_KEY}
ENV REDIS_CONNECTIONSTRING=${REDIS_CONNECTIONSTRING}

# Add health check
HEALTHCHECK --interval=10s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:5001/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "NetService.dll"]