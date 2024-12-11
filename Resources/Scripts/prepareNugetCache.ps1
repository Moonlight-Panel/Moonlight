# This script requires a NuGet override folder at %userprofile%\NugetOverride

# Clear old build cache
Remove-Item -Recurse -Force nupkgs, finalPackages

# Build and replace NuGet packages
& "Resources\Scripts\buildNuget.ps1"
Copy-Item -Path finalPackages\* -Destination $env:userprofile\NugetOverride -Force

# Clean package cache
Remove-Item -Recurse -Force $env:userprofile\.nuget\packages\moonlight.apiserver
Remove-Item -Recurse -Force $env:userprofile\.nuget\packages\moonlight.shared
Remove-Item -Recurse -Force $env:userprofile\.nuget\packages\moonlight.client

Write-Output "Done :>"
