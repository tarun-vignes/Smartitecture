param(
    [Parameter(Mandatory = $true)]
    [string]$BackendUrl,

    [Parameter(Mandatory = $true)]
    [string]$BackendApiKey,

    [string]$PackageName = "Smartitecture",

    [switch]$LaunchApp
)

$ErrorActionPreference = "Stop"

Write-Host "Smartitecture clean-machine QA"
Write-Host "Backend URL: $BackendUrl"
Write-Host ""

$healthUrl = "$($BackendUrl.TrimEnd('/'))/health"
$health = Invoke-RestMethod -Uri $healthUrl -Method Get -TimeoutSec 30
if ($health.status -ne "ok") {
    throw "Backend health check failed. Expected status 'ok', got '$($health.status)'."
}

Write-Host "PASS backend health: $($health.status)"

$chatUrl = "$($BackendUrl.TrimEnd('/'))/v1/chat"
$payload = @{
    messages = @(
        @{
            role = "user"
            content = "Reply with exactly: Smartitecture backend QA passed."
        }
    )
} | ConvertTo-Json -Depth 8

$chatResponse = Invoke-RestMethod `
    -Uri $chatUrl `
    -Method Post `
    -ContentType "application/json" `
    -Headers @{ "X-API-Key" = $BackendApiKey } `
    -Body $payload `
    -TimeoutSec 60

if ([string]::IsNullOrWhiteSpace($chatResponse.content)) {
    throw "Backend chat test returned an empty response."
}

Write-Host "PASS backend chat"
Write-Host $chatResponse.content
Write-Host ""

$package = Get-AppxPackage -Name $PackageName -ErrorAction SilentlyContinue
if ($null -eq $package) {
    Write-Warning "Smartitecture MSIX package is not installed for this user."
    Write-Host "Install it with Add-AppxPackage, then rerun this script."
} else {
    Write-Host "PASS package installed: $($package.PackageFullName)"
    Write-Host "Package location: $($package.InstallLocation)"
}

$startApp = Get-StartApps | Where-Object { $_.Name -eq "Smartitecture" } | Select-Object -First 1
if ($null -eq $startApp) {
    Write-Warning "Smartitecture Start Menu entry was not found."
} else {
    Write-Host "PASS Start Menu app identity: $($startApp.AppID)"

    if ($LaunchApp) {
        Start-Process "shell:AppsFolder\$($startApp.AppID)"
        Start-Sleep -Seconds 5
        $process = Get-Process Smartitecture -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($null -eq $process) {
            throw "Smartitecture did not appear to launch."
        }

        Write-Host "PASS app launched: PID $($process.Id)"
    }
}

Write-Host ""
Write-Host "Manual checks still required:"
Write-Host "  - Settings > AI Server test shows connected."
Write-Host "  - Ask: What is Google?"
Write-Host "  - Ask: Who is the president of the USA?"
Write-Host "  - Ask: Why is my PC slow?"
Write-Host "  - Ask: Scan results"
Write-Host "  - Switch language/theme and confirm text is readable."
