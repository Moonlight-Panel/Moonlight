FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Moonlight/Moonlight.csproj", "Moonlight/"]
RUN dotnet restore "Moonlight/Moonlight.csproj"
COPY . .
WORKDIR "/src/Moonlight"
RUN dotnet build "Moonlight.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Moonlight.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy default assets
RUN mkdir -p /app/Assets
COPY ./Moonlight/Assets ./Assets

ENTRYPOINT ["dotnet", "Moonlight.dll"]
