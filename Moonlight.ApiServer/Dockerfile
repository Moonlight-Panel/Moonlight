FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["Moonlight.ApiServer/Moonlight.ApiServer.csproj", "Moonlight.ApiServer/"]
COPY ["Moonlight.Client/Moonlight.Client.csproj", "Moonlight.Client/"]
COPY ["Moonlight.Shared/Moonlight.Shared.csproj", "Moonlight.Shared/"]

RUN dotnet restore "Moonlight.ApiServer/Moonlight.ApiServer.csproj"

COPY . .

WORKDIR "/src/Moonlight.ApiServer"

RUN dotnet build "Moonlight.ApiServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "Moonlight.ApiServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .

# Create storage directory a volume can bind to
RUN mkdir -p /app/storage

ENTRYPOINT ["dotnet", "Moonlight.ApiServer.dll"]
