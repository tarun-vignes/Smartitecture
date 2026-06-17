param(
    [string]$BackendUrl = "http://127.0.0.1:8080",
    [string]$BackendApiKey = ""
)

$ErrorActionPreference = "Stop"

Write-Host "Smartitecture smoke test"
Write-Host "Backend URL: $BackendUrl"

try {
    $health = Invoke-RestMethod -Method Get -Uri "$($BackendUrl.TrimEnd('/'))/health" -TimeoutSec 8
    Write-Host "PASS backend health: $($health.status)"
}
catch {
    Write-Host "WARN backend health failed: $($_.Exception.Message)"
}

$headers = @{}
if (-not [string]::IsNullOrWhiteSpace($BackendApiKey)) {
    $headers["Authorization"] = "Bearer $BackendApiKey"
    $headers["X-API-Key"] = $BackendApiKey
}

$body = @{
    messages = @(
        @{
            role = "user"
            content = "Reply with one short sentence confirming Smartitecture backend works."
        }
    )
    model = "smartitecture"
    tools = @()
} | ConvertTo-Json -Depth 8

try {
    $response = Invoke-RestMethod `
        -Method Post `
        -Uri "$($BackendUrl.TrimEnd('/'))/v1/chat" `
        -Headers $headers `
        -ContentType "application/json" `
        -Body $body `
        -TimeoutSec 30

    Write-Host "PASS backend chat"
    $response | ConvertTo-Json -Depth 8
}
catch {
    Write-Host "WARN backend chat failed: $($_.Exception.Message)"
    Write-Host "This is expected when the backend is not running, the backend API key is wrong, or GEMINI_API_KEY/OPENAI_API_KEY is not configured on the backend."
}

Write-Host ""
Write-Host "Desktop manual smoke checks:"
Write-Host "  1. Launch Smartitecture.exe."
Write-Host "  2. Open Settings > AI Server, enter $BackendUrl, then click Test."
Write-Host "  3. Ask: what can you do"
Write-Host "  4. Ask: why is my PC slow"
Write-Host "  5. Ask: show my IP"
Write-Host "  6. Ask: Defender status"
