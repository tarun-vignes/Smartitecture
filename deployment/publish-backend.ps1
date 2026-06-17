param(
    [string]$Configuration = "Release",
    [string]$OutputPath = "artifacts/backend",
    [switch]$Docker
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$backendProject = Join-Path $repoRoot "backend/Smartitecture.Backend/Smartitecture.Backend.csproj"
$publishDir = Join-Path $repoRoot $OutputPath

New-Item -ItemType Directory -Path $publishDir -Force | Out-Null

dotnet publish $backendProject `
    -c $Configuration `
    -o $publishDir `
    /p:UseAppHost=false

if ($Docker) {
    docker build `
        -t smartitecture-backend:local `
        (Join-Path $repoRoot "backend/Smartitecture.Backend")
}

Write-Host "Backend published to $publishDir"
Write-Host "Run locally with:"
Write-Host "  `$env:GEMINI_API_KEY='your-gemini-key'"
Write-Host "  dotnet Smartitecture.Backend.dll --urls http://127.0.0.1:8080"
