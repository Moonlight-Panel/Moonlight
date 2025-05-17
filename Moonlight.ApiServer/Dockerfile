#
# OUTDATED
# Use https://github.com/Moonlight-Panel/Deploy
#

# Prepare runtime docker image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled AS base
WORKDIR /app

# Prepare build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

# Install nodejs and npm so we can build tailwind
RUN apt-get update && apt-get install nodejs npm -y && apt-get clean

# Copy package.json file
WORKDIR /src
COPY ["Moonlight.Client/Styles/package.json", "Moonlight.Client/Styles/"]

# Install npm packages
WORKDIR /src/Moonlight.Client/Styles/
RUN npm i

# Copy project configuration files
WORKDIR /src
COPY ["Moonlight.ApiServer/Moonlight.ApiServer.csproj", "Moonlight.ApiServer/"]
COPY ["Moonlight.Client/Moonlight.Client.csproj", "Moonlight.Client/"]
COPY ["Moonlight.Shared/Moonlight.Shared.csproj", "Moonlight.Shared/"]

# Restore the nuget packages
RUN dotnet restore "Moonlight.ApiServer/Moonlight.ApiServer.csproj"

# Copy all files
COPY . .

# Build tailwindcss
WORKDIR "/src/Moonlight.Client/Styles"
RUN npx tailwindcss -i style.css -o ../wwwroot/css/core.min.css --minify

# Build ApiServer project
WORKDIR "/src/Moonlight.ApiServer"
RUN dotnet build "Moonlight.ApiServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish application
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "Moonlight.ApiServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Create final small image with the built content
FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Moonlight.ApiServer.dll"]
