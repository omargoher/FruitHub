# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY FruitHub.sln .
COPY FruitHub.API/FruitHub.API.csproj FruitHub.API/
COPY FruitHub.ApplicationCore/FruitHub.ApplicationCore.csproj FruitHub.ApplicationCore/
COPY FruitHub.Infrastructure/FruitHub.Infrastructure.csproj FruitHub.Infrastructure/

# Restore dependencies
RUN dotnet restore FruitHub.API/FruitHub.API.csproj

# Copy the rest of the source code
COPY . .

# Build and publish
RUN dotnet publish FruitHub.API/FruitHub.API.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Expose HTTP port
EXPOSE 8080

# Copy published output
COPY --from=build /app/publish .

# Start the application
ENTRYPOINT ["dotnet", "FruitHub.API.dll"]
