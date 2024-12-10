#!/bin/bash

# This script required a nuget override folder at ~/NugetOverride

# Clear old build cache
rm -rf nupkgs/ finalPackages/

# Build and replace nuget packages
bash Resources/Scripts/buildNuget.sh
cp finalPackages/* ~/NugetOverride/

# Clean package cache
rm -rf ~/.nuget/packages/moonlight.apiserver/
rm -rf ~/.nuget/packages/moonlight.shared/
rm -rf ~/.nuget/packages/moonlight.client/

echo "Done :>"