# Set strict mode to stop on errors
$ErrorActionPreference = "Stop"

# Ensure the script is running in the main directory
Write-Host "Building nuget packages"

Write-Host "Searching & building project files"
# Find all .csproj files recursively
$projectFiles = Get-ChildItem -Recurse -Filter "*.csproj"

foreach ($project in $projectFiles) {
    # Extract project name (without extension)
    $projectName = $project.BaseName

    # Extract version from the .csproj file
    $projectVersion = Select-String -Path $project.FullName -Pattern "<Version>(.*?)</Version>" | ForEach-Object {
        $_.Matches.Groups[1].Value
    }

    if (-not $projectVersion) {
        Write-Host "No <Version> tag found in $($project.FullName), skipping."
        continue
    }

    # Build and pack the project
    $projectPath = $project.DirectoryName
    $pwd = (Get-Location).Path
    Push-Location $projectPath
    dotnet build --configuration Release
    dotnet pack --configuration Release --output "$pwd\nupkgs"
    Pop-Location

    # Modifying the NuGet package
        Write-Host "Modding nuget package"
        $nugetPackage = Get-ChildItem "$pwd\nupkgs" -Filter "*.nupkg" | Select-Object -First 1
    
        # Rename .nupkg to .zip
        $zipPackage = "$($nugetPackage.FullName).zip"
        Rename-Item -Path $nugetPackage.FullName -NewName $zipPackage
    
        Expand-Archive -Path $zipPackage -DestinationPath "$pwd\nupkgs\mod" -Force
    
        if ($projectName -eq "Moonlight.ApiServer") {
            Remove-Item "$pwd\nupkgs\mod\content" -Recurse -Force -ErrorAction SilentlyContinue
            Remove-Item "$pwd\nupkgs\mod\contentFiles" -Recurse -Force -ErrorAction SilentlyContinue         
            
            $xmlFilePath = "$pwd\nupkgs\mod\$projectName.nuspec"
            $xmlContent = Get-Content -Path $xmlFilePath -Raw
            $regexPattern = '<contentFiles\b[^>]*>[\s\S]*?<\/contentFiles>'
            
            $updatedContent = [System.Text.RegularExpressions.Regex]::Replace(
                $xmlContent,
                $regexPattern,
                "",
                [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
            )
            
            Set-Content -Path $xmlFilePath -Value $updatedContent -Encoding UTF8
        }
    
        if ($projectName -eq "Moonlight.Client") {
            Remove-Item "$pwd\nupkgs\mod\staticwebassets\_framework" -Recurse -Force

            $xmlFilePath = "$pwd\nupkgs\mod\build\Microsoft.AspNetCore.StaticWebAssets.props"
            $xmlContent = Get-Content -Path $xmlFilePath -Raw
            
            $regexPattern = '<StaticWebAsset\b[^>]*>(?:(?!<\/StaticWebAsset>).)*?<RelativePath>_framework/blazor\.webassembly\.js(?:\.gz)?<\/RelativePath>.*?<\/StaticWebAsset>'
            
            $updatedContent = [System.Text.RegularExpressions.Regex]::Replace(
                $xmlContent,
                $regexPattern,
                "",
                [System.Text.RegularExpressions.RegexOptions]::Singleline
            )
            
            Set-Content -Path $xmlFilePath -Value $updatedContent -Encoding UTF8
        }
    
        # Repack the modified NuGet package
        Write-Host "Repacking nuget package"
        Remove-Item $zipPackage
        Push-Location "$pwd\nupkgs\mod"
        Compress-Archive -Path * -DestinationPath $zipPackage -Force
        Pop-Location
    
        # Rename .zip back to .nupkg
        Rename-Item -Path $zipPackage -NewName $nugetPackage.FullName

    # Move the final package to the output directory
    $finalDir = "$pwd\finalPackages"
    New-Item -ItemType Directory -Path $finalDir -Force | Out-Null
    Move-Item -Path $nugetPackage.FullName -Destination $finalDir

    # Cleanup
    Write-Host "Cleaning up"
    Remove-Item "$pwd\nupkgs\mod" -Recurse -Force
}
