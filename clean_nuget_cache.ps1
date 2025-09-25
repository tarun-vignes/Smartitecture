# Stop any running instances of Visual Studio
get-process -Name "devenv" -ErrorAction SilentlyContinue | Stop-Process -Force

# Clean NuGet cache folders
$nugetFolders = @(
    "$env:USERPROFILE\.nuget\packages",
    "$env:LOCALAPPDATA\NuGet\v3-cache",
    "$env:LOCALAPPDATA\Temp\NuGetScratch"
)

foreach ($folder in $nugetFolders) {
    if (Test-Path $folder) {
        Write-Host "Removing $folder..."
        Remove-Item -Path $folder -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "NuGet cache cleaned successfully."
