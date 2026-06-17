param(
    [string]$Publisher = "CN=Smartitecture",
    [string]$OutputPath = "artifacts/certs",
    [string]$Password = "SmartitectureDev123!"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$outputDir = Join-Path $repoRoot $OutputPath
$pfxPath = Join-Path $outputDir "Smartitecture-dev-signing.pfx"
$cerPath = Join-Path $outputDir "Smartitecture-dev-signing.cer"

New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

$cert = New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject $Publisher `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -NotAfter (Get-Date).AddYears(2)

$securePassword = ConvertTo-SecureString $Password -AsPlainText -Force
Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $securePassword | Out-Null
Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null

Write-Host "Development signing certificate created:"
Write-Host "  PFX: $pfxPath"
Write-Host "  CER: $cerPath"
Write-Host ""
Write-Host "Trust for local install:"
Write-Host "  Import-Certificate -FilePath '$cerPath' -CertStoreLocation Cert:\CurrentUser\TrustedPeople"
Write-Host ""
Write-Host "Package with:"
Write-Host "  .\deployment\package-msix.ps1 -CertificatePath '$pfxPath' -CertificatePassword '$Password'"
