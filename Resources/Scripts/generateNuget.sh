#!/bin/bash

# We are building the packages in the debug mode because they are meant for development
# purposes only. When build for production, release builds will be used ofc

set -e

echo "Creating nuget packages"
mkdir -p finalPackages

echo "+ ApiServer Server"
dotnet build -c Debug Moonlight.ApiServer
dotnet pack -c Debug Moonlight.ApiServer --output finalPackages/
dotnet run --project Resources/Scripts/Scripts.csproj content finalPackages/Moonlight.ApiServer.2.1.0.nupkg ".*"

echo "+ Client"
dotnet build -c Debug Moonlight.Client
dotnet pack -c Debug Moonlight.Client --output finalPackages/
dotnet run --project Resources/Scripts/Scripts.csproj staticWebAssets finalPackages/Moonlight.Client.2.1.0.nupkg "_framework\/.*" style.min.css
dotnet run --project Resources/Scripts/Scripts.csproj src finalPackages/Moonlight.Client.2.1.0.nupkg Moonlight.Client/ *.razor
dotnet run --project Resources/Scripts/Scripts.csproj src finalPackages/Moonlight.Client.2.1.0.nupkg Moonlight.Client/ wwwroot/*.html

echo "+ Shared library"
dotnet build -c Debug Moonlight.Shared
dotnet pack -c Debug Moonlight.Shared --output finalPackages/