#!/bin/bash
set -e

# Note: Run in main directory, i.e. where the Moonlight.sln is

echo "Building nuget packages"

echo "Searching & building project files"
project_files=$(find . -name "*.csproj")

for project in $project_files; do
    # Extract project name
    project_name=$(basename "$project" .csproj)

    # Extract version
    project_version=$(grep -oPm1 "(?<=<Version>)[^<]+" "$project")
    if [ -z "$project_version" ]; then
        echo "No <Version> tag found in $project, skipping."
        continue
    fi

    # Building nuget package
    pwd=$(pwd)
    project_path=$(dirname $project)
    (cd $project_path; dotnet build --configuration Release; dotnet pack --configuration Release --output $pwd/nupkgs)
    
    # Mod nuget package
    echo "Modding nuget package"
    nugetPackage=$(find $pwd/nupkgs -name "*.nupkg")
    
    unzip -o $nugetPackage -d $pwd/nupkgs/mod
    
    if [ "$project_name" = "Moonlight.ApiServer" ]; then
      rm -r $pwd/nupkgs/mod/content
      rm -r $pwd/nupkgs/mod/contentFiles
      
      sed -i "/<contentFiles>/,/<\/contentFiles>/d" $pwd/nupkgs/mod/Moonlight.ApiServer.nuspec
    fi
    
    if [ "$project_name" = "Moonlight.Client" ]; then
      rm -r $pwd/nupkgs/mod/staticwebassets/_framework
          
      sed -i '/<StaticWebAsset Include=.*blazor.webassembly.js.gz.*>/,/<\/StaticWebAsset>/d' $pwd/nupkgs/mod/build/Microsoft.AspNetCore.StaticWebAssets.props
      sed -i '/<StaticWebAsset Include=.*blazor.webassembly.js.*>/,/<\/StaticWebAsset>/d' $pwd/nupkgs/mod/build/Microsoft.AspNetCore.StaticWebAssets.props
          
    fi
    
    echo "Repacking nuget package"
    rm $nugetPackage
    (cd nupkgs/mod/; zip -r -o $nugetPackage *)
    
    mkdir -p $pwd/finalPackages/
    
    mv $nugetPackage $pwd/finalPackages/
    
    echo "Cleaning up"
    rm -r $pwd/nupkgs/mod
done