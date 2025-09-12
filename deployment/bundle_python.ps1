 # PowerShell script to bundle Python runtime with the application
 # This creates a self-contained deployment package
 
 param(
     [string]$Configuration = "Release",
     [string]$Runtime = "win-x64"
 )
 
 # Resolve repository root relative to this script (script is in /deployment)
 $ScriptDir = $PSScriptRoot
 $RepoRoot = (Resolve-Path (Join-Path $ScriptDir "..")).Path
 Write-Host "Script directory: $ScriptDir" -ForegroundColor DarkGray
 Write-Host "Repo root: $RepoRoot" -ForegroundColor DarkGray
 
 Write-Host "Creating self-contained Smartitecture desktop application..." -ForegroundColor Green
 
 # Create deployment directory
 $deployDir = (Join-Path $RepoRoot "deploy\Smartitecture-Desktop")
 if (Test-Path $deployDir) {
     Write-Host "Cleaning previous deployment at $deployDir" -ForegroundColor DarkYellow
     Remove-Item $deployDir -Recurse -Force
 }
 New-Item -ItemType Directory -Path $deployDir -Force | Out-Null
 
 # Build the .NET application as self-contained
 $projectFile = (Join-Path $RepoRoot "Smartitecture.csproj")
 $publishOut = (Join-Path $deployDir "app")
 Write-Host "Building .NET application..." -ForegroundColor Yellow
 Write-Host "Project: $projectFile" -ForegroundColor DarkGray
 Write-Host "Output:  $publishOut" -ForegroundColor DarkGray
 
 dotnet publish "$projectFile" -c $Configuration -r $Runtime --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "$publishOut"
 
 if ($LASTEXITCODE -ne 0) {
     Write-Error "Failed to build .NET application"
     exit 1
 }
 
 # Create Python runtime directory
 $pythonDir = (Join-Path $deployDir "python")
 New-Item -ItemType Directory -Path $pythonDir -Force | Out-Null
 
 # Copy minimal Python backend
 $backendPy = (Join-Path $RepoRoot "backend-python\minimal_server.py")
 Write-Host "Bundling Python backend..." -ForegroundColor Yellow
 if (Test-Path $backendPy) {
     Copy-Item "$backendPy" (Join-Path $pythonDir "minimal_server.py") -Force
 } else {
     Write-Warning "Backend python not found at $backendPy"
 }
 
 # Create embedded Python runtime (using python embeddable package)
 $pythonEmbedUrl = "https://www.python.org/ftp/python/3.11.0/python-3.11.0-embed-amd64.zip"
 $pythonZip = (Join-Path $deployDir "python-embed.zip")
 
 Write-Host "Downloading Python embeddable runtime..." -ForegroundColor Yellow
 try {
     Invoke-WebRequest -Uri $pythonEmbedUrl -OutFile $pythonZip
     Expand-Archive -Path $pythonZip -DestinationPath (Join-Path $pythonDir "runtime") -Force
     Remove-Item $pythonZip
 } catch {
     Write-Warning "Could not download Python runtime. Using system Python instead. Error: $($_.Exception.Message)"
 }
 
 # Create launcher script
 $launcherScript = @"
 @echo off
 echo Starting Smartitecture Desktop Application...
 
 REM Start the main application
 start "" "%~dp0app\Smartitecture.exe"
 
echo Smartitecture is starting...
echo Check your system tray or taskbar for the application window.
pause
"@

$launcherScript | Out-File -FilePath (Join-Path $deployDir "Start-Smartitecture.bat") -Encoding ASCII

# Create application info
$appInfo = @"
# Smartitecture Desktop Application

## What's Included
- Smartitecture.exe - Main WPF application
- Python runtime and backend server
- All dependencies bundled

## How to Run
1. Double-click Start-Smartitecture.bat
2. The application will launch automatically
3. Python backend starts embedded within the application

## System Requirements
- Windows 10/11 (x64)
- No additional software required (fully self-contained)

## Architecture
- Native Windows UI (WPF/C#)
- Embedded Python AI backend
- Local HTTP communication
- No internet connection required

Built on: $(Get-Date)
"@

$appInfo | Out-File -FilePath (Join-Path $deployDir "README.txt") -Encoding UTF8

Write-Host "Deployment package created successfully!" -ForegroundColor Green
Write-Host "Location: $deployDir" -ForegroundColor Cyan
Write-Host "To distribute: Zip the entire '$deployDir' folder" -ForegroundColor Cyan
