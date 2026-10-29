param(
    [ValidateSet('win-x64', 'win-arm64')]
    [string]$Runtime = 'win-x64',
    [ValidatePattern('^\d+\.\d+\.\d+\.\d+$')]
    [string]$Version = '1.0.0.6',
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$RuntimeFrameworkVersion = '8.0.33'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$buildId = [guid]::NewGuid().ToString('N').Substring(0, 8)
$releaseDir = Join-Path $repoRoot "artifacts/public-preview/$Version-$Runtime-$buildId"
$appDir = Join-Path $releaseDir 'Smartitecture'
New-Item -ItemType Directory -Path $appDir -Force | Out-Null

# Every attempt uses an empty output directory so old binaries cannot enter a release.
dotnet publish (Join-Path $repoRoot 'Smartitecture.csproj') `
    -c Release -r $Runtime --self-contained true -o $appDir `
    /p:PublishSingleFile=false /p:PublishTrimmed=false /p:Version=$Version `
    /p:SmartitectureRuntimeVersion=$RuntimeFrameworkVersion
if ($LASTEXITCODE -ne 0) {
    throw "Publish failed ($LASTEXITCODE). This attempt is incomplete; no archive will be created."
}

foreach ($required in @('Smartitecture.exe', 'Smartitecture.dll', 'coreclr.dll', 'hostfxr.dll', 'PresentationFramework.dll', 'appsettings.json')) {
    if (-not (Test-Path (Join-Path $appDir $required))) {
        throw "Release is incomplete: missing $required."
    }
}

$config = Get-Content (Join-Path $appDir 'appsettings.json') -Raw | ConvertFrom-Json
foreach ($provider in @('OpenAI', 'AzureOpenAI', 'Anthropic', 'Gemini', 'Backend', 'Agent')) {
    $section = $config.$provider
    if ($null -eq $section) { continue }
    foreach ($property in $section.PSObject.Properties) {
        if ($property.Name -match 'ApiKey|Secret|Password|^(Access|Refresh|Id)?Token$' -and -not [string]::IsNullOrWhiteSpace([string]$property.Value)) {
            throw "Release configuration contains a credential in $provider.$($property.Name). No archive was created."
        }
    }
}

@'
SMARTITECTURE - WINDOWS PREVIEW

GET STARTED
1. Extract the entire ZIP into a folder on your PC.
2. Open the Smartitecture folder and double-click Smartitecture.exe.
3. Choose Check my PC, or ask "Why is my PC slow?"

You do not need to install .NET, an SDK, or development tools.
Keep all the extracted files together. Do not run the app from inside the ZIP.

WHAT WORKS WITHOUT SETUP
PC information, performance checks, battery information, network information,
supported Windows Security checks, and opening familiar Windows apps.
Some checks depend on Windows permissions and hardware support.

ONLINE AI
Local help is ready without an account. Online AI requires an available service
and account access. Production account service setup is still pending; this
preview does not promise general online AI access.

IF SOMETHING GOES WRONG
This is preview software. It is not a signed public installer.
If Windows blocks it, do not disable Windows security or install development
certificates. Report the issue and wait for a trusted distribution package.

Settings and logs are stored under your Windows user profile:
%LOCALAPPDATA%\Smartitecture
For help, use Settings > Support > Copy diagnostics.
Review diagnostics before sharing them. Never include passwords or access keys.
'@ | Set-Content (Join-Path $appDir 'START HERE.txt') -Encoding UTF8

$archive = Join-Path $releaseDir "Smartitecture-$Version-$Runtime-preview.zip"
Compress-Archive -Path $appDir -DestinationPath $archive
$hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
@{
    version = $Version
    runtime = $Runtime
    runtimeFrameworkVersion = $RuntimeFrameworkVersion
    selfContained = $true
    signed = $false
    onlineAiReady = $false
    createdAt = [DateTimeOffset]::UtcNow.ToString('o')
    archive = [IO.Path]::GetFileName($archive)
    sha256 = $hash
} | ConvertTo-Json | Set-Content (Join-Path $releaseDir 'release.json') -Encoding UTF8
Write-Host "Preview built: $archive"
Write-Host "SHA256: $hash"
Write-Host 'Do not call this a public release until launch, clean-machine QA, and trusted distribution are verified.'
