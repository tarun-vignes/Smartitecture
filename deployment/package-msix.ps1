param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "1.0.0.5",
    [string]$Publisher = "CN=Smartitecture",
    [string]$OutputPath = "artifacts/msix",
    [string]$CertificatePath = "",
    [string]$CertificatePassword = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $repoRoot "artifacts/desktop"
$stageDir = Join-Path $repoRoot "artifacts/msix-stage"
$outputDir = Join-Path $repoRoot $OutputPath
$packagePath = Join-Path $outputDir "Smartitecture-$Version-$Runtime.msix"
$manifestSource = Join-Path $repoRoot "Package.appxmanifest"
$manifestTarget = Join-Path $stageDir "AppxManifest.xml"
$logoSource = Join-Path $repoRoot "Assets/Logo"
$logoTarget = Join-Path $stageDir "Assets/Logo"

function Find-WindowsSdkTool {
    param([Parameter(Mandatory)][string]$ToolName)

    $kitRoot = "${env:ProgramFiles(x86)}\Windows Kits\10\bin"
    if (-not (Test-Path $kitRoot)) {
        throw "Windows SDK bin folder was not found. Install Windows 10/11 SDK to use $ToolName."
    }

    $tool = Get-ChildItem $kitRoot -Recurse -Filter $ToolName -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match "\\x64\\$([regex]::Escape($ToolName))$" } |
        Sort-Object FullName -Descending |
        Select-Object -First 1

    if ($null -eq $tool) {
        throw "$ToolName was not found under $kitRoot."
    }

    return $tool.FullName
}

New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

& (Join-Path $PSScriptRoot "publish-desktop.ps1") `
    -Configuration $Configuration `
    -Runtime $Runtime `
    -OutputPath "artifacts/desktop"

if (Test-Path $stageDir) {
    Remove-Item $stageDir -Recurse -Force
}

New-Item -ItemType Directory -Path $stageDir -Force | Out-Null
Copy-Item -Path (Join-Path $publishDir "*") -Destination $stageDir -Recurse -Force
New-Item -ItemType Directory -Path $logoTarget -Force | Out-Null

$logoFiles = @(
    "AppIcon.ico",
    "BadgeLogo.png",
    "LargeTile310x310.png",
    "Logo150x150.png",
    "Logo310x310.png",
    "Logo44x44.png",
    "Logo44x44.targetsize-16.png",
    "Logo44x44.targetsize-16.altform-unplated.png",
    "Logo44x44.targetsize-24.png",
    "Logo44x44.targetsize-24.altform-unplated.png",
    "Logo44x44.targetsize-32.png",
    "Logo44x44.targetsize-32.altform-unplated.png",
    "Logo44x44.targetsize-48.png",
    "Logo44x44.targetsize-48.altform-unplated.png",
    "Logo44x44.targetsize-64.png",
    "Logo44x44.targetsize-64.altform-unplated.png",
    "Logo44x44.targetsize-128.png",
    "Logo44x44.targetsize-128.altform-unplated.png",
    "Logo44x44.targetsize-256.png",
    "Logo44x44.targetsize-256.altform-unplated.png",
    "Logo50x50.png",
    "Logo620x300.png",
    "Logo71x71.png",
    "SmallTile71x71.png",
    "SmartitectureApp.ico",
    "SplashScreen620x300.png",
    "StoreLogo.png"
)

foreach ($logo in $logoFiles) {
    $sourcePath = Join-Path $logoSource $logo
    if (-not (Test-Path $sourcePath)) {
        throw "Required MSIX logo asset was not found: $sourcePath"
    }

    Copy-Item -Path $sourcePath -Destination (Join-Path $logoTarget $logo) -Force
}

[xml]$manifest = Get-Content $manifestSource -Raw
$manifest.Package.Identity.Version = $Version
$manifest.Package.Identity.Publisher = $Publisher

$application = $manifest.Package.Applications.Application
$application.Executable = "Smartitecture.exe"
$application.EntryPoint = "Windows.FullTrustApplication"

$manifest.Save($manifestTarget)

$makeAppx = Find-WindowsSdkTool -ToolName "makeappx.exe"
if (Test-Path $packagePath) {
    Remove-Item $packagePath -Force
}

& $makeAppx pack /d $stageDir /p $packagePath /o
if ($LASTEXITCODE -ne 0) {
    throw "makeappx failed with exit code $LASTEXITCODE."
}

if (-not [string]::IsNullOrWhiteSpace($CertificatePath)) {
    if (-not (Test-Path $CertificatePath)) {
        throw "Certificate file not found: $CertificatePath"
    }

    $signTool = Find-WindowsSdkTool -ToolName "signtool.exe"
    $signArgs = @("sign", "/fd", "SHA256", "/f", $CertificatePath)
    if (-not [string]::IsNullOrWhiteSpace($CertificatePassword)) {
        $signArgs += @("/p", $CertificatePassword)
    }

    $signArgs += $packagePath
    & $signTool @signArgs
    if ($LASTEXITCODE -ne 0) {
        throw "signtool failed with exit code $LASTEXITCODE."
    }
}

Write-Host "MSIX package created:"
Write-Host "  $packagePath"
Write-Host ""
Write-Host "Install locally with:"
Write-Host "  Add-AppxPackage '$packagePath'"
Write-Host ""
Write-Host "If the package is signed with a self-signed cert, install/trust the certificate first."
