FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["Moonlight/Moonlight.csproj", "Moonlight/"]
RUN dotnet restore "Moonlight/Moonlight.csproj"
COPY . .
WORKDIR "/src/Moonlight"
RUN dotnet build "Moonlight.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Install sass
RUN apt-get update
RUN apt-get install wget -y
RUN cat /etc/os-release
RUN wget -O /tmp/sass.tar.gz https://github.com/sass/dart-sass/releases/download/1.77.5/dart-sass-1.77.5-linux-x64.tar.gz
RUN tar -xf /tmp/sass.tar.gz -C /tmp
RUN chmod +x /tmp/dart-sass/sass
RUN mkdir -p /app/build/Assets/Core/css/
RUN /tmp/dart-sass/sass /src/Moonlight/Styles/style.scss /app/build/Assets/Core/css/theme.css

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Moonlight.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final

# Define args
ARG BUILD_CHANNEL=unknown
ARG BUILD_COMMIT_HASH=unknown
ARG BUILD_NAME=unknown
ARG BUILD_VERSION=unknown

WORKDIR /app
COPY --from=publish /app/publish .

# Copy default assets
RUN mkdir -p /app/Assets
COPY ./Moonlight/Assets ./Assets

# Ensure storage folder exists and is empty
RUN mkdir -p /app/storage
RUN rm -rf /app/storage/*

# Version the build
RUN echo "$BUILD_CHANNEL;$BUILD_COMMIT_HASH;$BUILD_NAME;$BUILD_VERSION;docker" > /app/version

ENTRYPOINT ["dotnet", "Moonlight.dll"]
