# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Skill-Loop.Api/Skill-Loop.Api.csproj", "src/Skill-Loop.Api/"]
COPY ["src/Skill-Loop.Application/Skill-Loop.Application.csproj", "src/Skill-Loop.Application/"]
COPY ["src/Skill-Loop.Domain/Skill-Loop.Domain.csproj", "src/Skill-Loop.Domain/"]
COPY ["src/Skill-Loop.Infrastructure/Skill-Loop.Infrastructure.csproj", "src/Skill-Loop.Infrastructure/"]
RUN dotnet restore "./src/Skill-Loop.Api/Skill-Loop.Api.csproj"
COPY . .
WORKDIR "/src/src/Skill-Loop.Api"
RUN dotnet build "./Skill-Loop.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Skill-Loop.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create the upload root up front and hand it to the non-root app user.
# docker-compose mounts a named volume at /app/UploadedFiles; an empty volume is
# created root-owned, so without this the app cannot write uploads.
USER root
RUN mkdir -p /app/UploadedFiles && chown -R $APP_UID:$APP_UID /app/UploadedFiles
USER $APP_UID

ENTRYPOINT ["dotnet", "Skill-Loop.Api.dll"]