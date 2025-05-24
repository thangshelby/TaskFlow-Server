# # Use the official .NET SDK image to build the project
# FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# WORKDIR /app

# # Copy the .csproj file(s) and restore dependencies
# COPY *.csproj ./
# RUN dotnet restore

# # Copy the rest of the project files
# COPY . ./
# RUN dotnet publish NetService.csproj -c Release -o out
# RUN ls out/*.dll  # Print DLL files

# # Use the .NET runtime image for the final image
# FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
# WORKDIR /app
# COPY --from=build /app/out ./

# # Expose the gRPC port
# EXPOSE 5001

# # Set the entry point to run the gRPC server
# ENTRYPOINT ["dotnet", "NetService.dll"]


# Dockerfile.dev
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS development
WORKDIR /app

# Copy the .csproj file(s) and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the project files
COPY . ./

# Expose the gRPC port
EXPOSE 5001

# Use dotnet watch for hot reload
ENTRYPOINT ["dotnet", "watch", "--project", "NetService.csproj", "run", "--urls", "http://0.0.0.0:5001"]