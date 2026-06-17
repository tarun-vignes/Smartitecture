param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputPath = "artifacts/desktop",
    [switch]$SelfContained,
    [switch]$Zip
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "Smartitecture.csproj"
$publishDir = Join-Path $repoRoot $OutputPath

New-Item -ItemType Directory -Path $publishDir -Force | Out-Null

$selfContainedValue = if ($SelfContained) { "true" } else { "false" }

dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained $selfContainedValue `
    -o $publishDir `
    /p:PublishSingleFile=false

if ($Zip) {
    $zipPath = Join-Path $repoRoot "artifacts/Smartitecture-$Runtime.zip"
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }

    Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath
    Write-Host "Desktop package zipped to $zipPath"
}

Write-Host "Desktop app published to $publishDir"
Write-Host "Entry point:"
Write-Host "  $(Join-Path $publishDir 'Smartitecture.exe')"
