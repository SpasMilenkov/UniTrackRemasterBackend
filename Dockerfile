# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

EXPOSE 5086

# Base image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and projects to restore dependencies
COPY UniTrackRemaster.sln ./
COPY UniTrackRemaster.Api/UniTrackRemaster.Api.csproj UniTrackRemaster.Api/
COPY UniTrackRemaster.Api.Dto/UniTrackRemaster.Api.Dto.csproj UniTrackRemaster.Api.Dto/
COPY UniTrackRemaster.Data/UniTrackRemaster.Data.csproj UniTrackRemaster.Data/
COPY UniTrackRemaster.Data.Commons/UniTrackRemaster.Data.Commons.csproj UniTrackRemaster.Data.Commons/
COPY UniTrackRemaster.Data.Models/UniTrackRemaster.Data.Models.csproj UniTrackRemaster.Data.Models/
COPY UniTrackRemaster.Data.Repositories/UniTrackRemaster.Data.Repositories.csproj UniTrackRemaster.Data.Repositories/
COPY UniTrackRemaster.Infrastructure/UniTrackRemaster.Infrastructure.csproj UniTrackRemaster.Infrastructure/
COPY UniTrackRemaster.Mappings/UniTrackRemaster.Mappings.csproj UniTrackRemaster.Mappings/
COPY UniTrackRemaster.Messaging/UniTrackRemaster.Messaging.csproj UniTrackRemaster.Messaging/
COPY UniTrackRemaster.Services.Authentication/UniTrackRemaster.Services.Authentication.csproj UniTrackRemaster.Services.Authentication/
COPY OrganizationServices/OrganizationServices.csproj OrganizationServices/
COPY StorageService/StorageService.csproj StorageService/

# Restore dependencies
RUN dotnet restore UniTrackRemaster.Api/UniTrackRemaster.Api.csproj

# Copy all source code
COPY . ./

# Build the application
WORKDIR /src/UniTrackRemaster.Api
RUN dotnet build UniTrackRemaster.Api.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish UniTrackRemaster.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UniTrackRemaster.Api.dll"]
