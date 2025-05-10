#!/bin/bash

bash Resources/Scripts/generateNuget.sh

echo "+ Copying to nuget override"
cp finalPackages/*.nupkg ~/NugetOverride/
rm -r ~/.nuget/packages/moonlight.*