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

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

# Install sass and compile styles
RUN apt-get update
RUN apt-get install wget -y

RUN ARCH=$(uname -m) && \
    if [ "$ARCH" = "x86_64" ]; then \
        wget -O /tmp/sass.tar.gz https://github.com/sass/dart-sass/releases/download/1.77.5/dart-sass-1.77.5-linux-x64.tar.gz; \
    elif [ "$ARCH" = "aarch64" ]; then \
        wget -O /tmp/sass.tar.gz https://github.com/sass/dart-sass/releases/download/1.77.5/dart-sass-1.77.5-linux-arm64.tar.gz; \
    else \
        echo "Unsupported architecture: $ARCH"; exit 1; \
    fi

RUN tar -xf /tmp/sass.tar.gz -C /tmp
RUN chmod +x /tmp/dart-sass/sass
RUN /tmp/dart-sass/sass /src/Moonlight/Styles/style.scss /app/publish/theme.css

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
COPY ./Moonlight/Assets /app/Assets
RUN mv /app/theme.css /app/Assets/Core/css/theme.css

# Ensure storage folder exists and is empty
RUN mkdir -p /app/storage
RUN rm -rf /app/storage/*

# Version the build
RUN echo "$BUILD_CHANNEL;$BUILD_COMMIT_HASH;$BUILD_NAME;$BUILD_VERSION;docker" > /app/version

ENTRYPOINT ["dotnet", "Moonlight.dll"]
